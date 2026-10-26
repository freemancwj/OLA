using System;
using System.Text.RegularExpressions;

namespace WaterCloud.Code
{
	/// <summary>
	/// 数据库方言适配。
	/// 系统内的原生 SQL 统一按 SqlServer 语法书写，运行时根据当前数据库类型转换为目标数据库语法。
	/// 支持 PostgreSQL、SQLite 与 SqlServer 的差异（ISNULL→COALESCE、TOP→LIMIT、字符串拼接、布尔值、GETDATE、NEWID、CONVERT 等）。
	/// </summary>
	public static class SqlDialect
	{
		/// <summary>
		/// 实体中定义为 bool 的列名（PostgreSQL 中对应 boolean 类型，不能与 0/1 直接比较）
		/// </summary>
		private static readonly string[] BoolColumns =
		{
			"F_DeleteMark", "F_EnabledMark", "F_IsAdmin", "F_IsPublic", "F_IsFields", "F_IsMenu", "F_IsTree",
			"F_IsDefault", "F_IsExpand", "F_IsTop", "F_IsHot", "F_IsRed", "F_IsFinish", "F_IsLeaderInDepts",
			"F_IsSenior", "F_IsBoss", "F_IsSelected", "F_MultiUserLogin", "F_CheckIPAddress", "F_AllowDelete",
			"F_AllowEdit", "F_ClickRead", "F_DoItNow", "F_IncludeStat", "F_LastRunMark", "F_Result", "F_Split",
			"F_TransitionSate", "F_Batch", "F_UserOnLine", "F_Gender"
		};

		private static readonly string BoolPattern = string.Join("|", BoolColumns);

		/// <summary>
		/// MySQL 字符串拼接链中的操作数（字符串字面量、COALESCE 调用、列名）
		/// </summary>
		private const string ConcatOperand = @"(?:COALESCE\s*\([^()]*\)|'[^']*'|[\w\.]+)";

		/// <summary>
		/// 当前数据库类型（小写）
		/// </summary>
		public static string CurrentDbType
		{
			get
			{
				var provider = GlobalContext.SystemConfig == null ? null : GlobalContext.SystemConfig.DBProvider;
				return string.IsNullOrEmpty(provider) ? "sqlserver" : provider.Trim().ToLowerInvariant();
			}
		}

		/// <summary>
		/// 当前是否是 PostgreSQL 数据库
		/// </summary>
		public static bool IsPostgreSql => CurrentDbType.Contains("postgre") || CurrentDbType.Contains("pgsql");

		/// <summary>
		/// 当前是否是 SQLite 数据库
		/// </summary>
		public static bool IsSqlite => CurrentDbType.Contains("sqlite");

		/// <summary>
		/// 当前是否是 MySQL 数据库
		/// </summary>
		public static bool IsMySql => CurrentDbType.Contains("mysql");

		/// <summary>
		/// 是否需要方言转换（非 SqlServer 的数据库都需要）
		/// </summary>
		public static bool NeedTrans => IsPostgreSql || IsSqlite || IsMySql;

		/// <summary>
		/// 布尔常量（用于拼接原生 SQL 的默认值）
		/// </summary>
		public static string Bool(bool value)
		{
			if (IsPostgreSql)
			{
				return value ? "TRUE" : "FALSE";
			}
			// SQLite 原生 SQL 中布尔值用 1/0
			return value ? "1" : "0";
		}

		/// <summary>
		/// SELECT 后的 TOP 关键字（PostgreSQL/SQLite 使用 LIMIT，由 <see cref="Limit"/> 补在语句末尾）
		/// </summary>
		public static string Top(int count)
		{
			return NeedTrans ? string.Empty : "TOP " + count + " ";
		}

		/// <summary>
		/// 语句末尾的 LIMIT 关键字（PostgreSQL/SQLite）
		/// </summary>
		public static string Limit(int count)
		{
			return NeedTrans ? " LIMIT " + count : string.Empty;
		}

		/// <summary>
		/// 把 SqlServer 语法的原生 SQL 转换为当前数据库语法
		/// </summary>
		public static string Trans(string sql)
		{
			if (string.IsNullOrEmpty(sql) || !NeedTrans)
			{
				return sql;
			}
			var result = sql;
			// 排序规则（PostgreSQL、SQLite 都不支持）
			result = Regex.Replace(result, @"\s+COLLATE\s+[\w]+", string.Empty, RegexOptions.IgnoreCase);
			// Unicode 字符串前缀 N'
			result = Regex.Replace(result, @"(?<![\w'])N'", "'", RegexOptions.IgnoreCase);
			// GETDATE() / GETUTCDATE() → 当前时间
			result = Regex.Replace(result, @"\bGETDATE\s*\(\s*\)", IsSqlite ? "datetime('now','localtime')" : "now()", RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"\bGETUTCDATE\s*\(\s*\)", IsSqlite ? "datetime('now')" : "now()", RegexOptions.IgnoreCase);
			// NEWID() → 新 GUID
			if (IsSqlite)
			{
				result = Regex.Replace(result, @"\bNEWID\s*\(\s*\)", "lower(hex(randomblob(16)))", RegexOptions.IgnoreCase);
			}
			else if (IsMySql)
			{
				result = Regex.Replace(result, @"\bNEWID\s*\(\s*\)", "UUID()", RegexOptions.IgnoreCase);
			}
			else
			{
				result = Regex.Replace(result, @"\bNEWID\s*\(\s*\)", "gen_random_uuid()", RegexOptions.IgnoreCase);
			}
			// TOP n SELECT ... → 换成 LIMIT
			result = Regex.Replace(result, @"\bTOP\s+(\d+)\b", "", RegexOptions.IgnoreCase);
			// CONVERT(bit,0/1)
			result = Regex.Replace(result, @"CONVERT\s*\(\s*bit\s*,\s*1\s*\)", IsPostgreSql ? "TRUE" : "1", RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"CONVERT\s*\(\s*bit\s*,\s*0\s*\)", IsPostgreSql ? "FALSE" : "0", RegexOptions.IgnoreCase);
			// CONVERT(varchar(n), expr, 108)：时间列本身已是字符串，直接取原值
			result = Regex.Replace(result, @"CONVERT\s*\(\s*varchar\s*\(\s*\d+\s*\)\s*,\s*([\w\.]+)\s*,\s*108\s*\)", "$1", RegexOptions.IgnoreCase);
			// CONVERT(varchar(n), expr, 23)：日期格式化
			if (IsPostgreSql)
			{
				result = Regex.Replace(result, @"CONVERT\s*\(\s*varchar\s*\(\s*\d+\s*\)\s*,\s*([\w\.]+)\s*,\s*23\s*\)", "to_char($1,'YYYY-MM-DD')", RegexOptions.IgnoreCase);
			}
			else
			{
				result = Regex.Replace(result, @"CONVERT\s*\(\s*varchar\s*\(\s*\d+\s*\)\s*,\s*([\w\.]+)\s*,\s*23\s*\)", "date($1)", RegexOptions.IgnoreCase);
			}
			// 字符串拼接：+ 换成 ||（MySQL 后续再把 || 链转 CONCAT）
			result = Regex.Replace(result, @"'\s*\+\s*", "' || ");
			result = Regex.Replace(result, @"\s*\+\s*'", " || '");
			// ISNULL 换成 COALESCE（两者语义相同，但统一处理方便后续布尔列转换）
			result = Regex.Replace(result, @"\bISNULL\s*\(", "COALESCE(", RegexOptions.IgnoreCase);
			// SQL Server SET QUOTED_IDENTIFIER / NOCOUNT / ANSI_NULLS 等控制语句 — SQLite/PostgreSQL/MySQL 不支持
			result = Regex.Replace(result, @"SET\s+QUOTED_IDENTIFIER\s+(ON|OFF)\s*;?", string.Empty, RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"SET\s+NOCOUNT\s+(ON|OFF)\s*;?", string.Empty, RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"SET\s+ANSI_NULLS\s+(ON|OFF)\s*;?", string.Empty, RegexOptions.IgnoreCase);
			if (IsMySql)
			{
				// MySQL 中 || 是逻辑或，字符串拼接需用 CONCAT：把连续的 操作数 || 操作数 链包成 CONCAT(...)
				result = Regex.Replace(result, ConcatOperand + @"(?:\s*\|\|\s*" + ConcatOperand + @")+",
					match =>
					{
						var parts = Regex.Split(match.Value, @"\s*\|\|\s*");
						return "CONCAT(" + string.Join(", ", parts) + ")";
					}, RegexOptions.IgnoreCase);
			}
			if (IsPostgreSql)
			{
				// 布尔列默认值：COALESCE(布尔列,0/1) → PostgreSQL 需要 TRUE/FALSE
				result = Regex.Replace(result, @"(COALESCE\s*\(\s*(?:[\w]+\.)?(?:" + BoolPattern + @")\s*,\s*)0(\s*\))",
					"$1FALSE$2", RegexOptions.IgnoreCase);
				result = Regex.Replace(result, @"(COALESCE\s*\(\s*(?:[\w]+\.)?(?:" + BoolPattern + @")\s*,\s*)1(\s*\))",
					"$1TRUE$2", RegexOptions.IgnoreCase);
				// 布尔表达式与 0/1 的比较：COALESCE(...) = 0 → PostgreSQL 需要布尔直接比较
				result = Regex.Replace(result, @"(COALESCE\s*\(\s*(?:[\w]+\.)?(?:" + BoolPattern + @")\s*,\s*(?:TRUE|FALSE)\s*\))\s*(=|<>|!=)\s*([01])\b",
					MatchBoolCompare, RegexOptions.IgnoreCase);
				// 布尔列与 0/1 的比较：a.F_DeleteMark = 0
				result = Regex.Replace(result, @"((?:[\w]+\.)?(?:" + BoolPattern + @"))\s*(=|<>|!=)\s*([01])\b",
					MatchBoolCompare, RegexOptions.IgnoreCase);
			}
			return result;
		}

		private static string MatchBoolCompare(Match match)
		{
			var value = match.Groups[3].Value == "1" ? "TRUE" : "FALSE";
			return match.Groups[1].Value + " " + match.Groups[2].Value + " " + value;
		}

		/// <summary>
		/// 把实体的 CLR 类型（结合 SqlServer 列类型中声明的长度/精度）转换为 PostgreSQL 列类型。
		/// 以 CLR 类型为准，避免 tinyint/bit 等 SqlServer 声明与 PostgreSQL 实际类型不一致。
		/// </summary>
		public static string ToPostgreSqlType(Type clrType, string sqlServerType)
		{
			var type = Nullable.GetUnderlyingType(clrType) ?? clrType;
			if (type == typeof(bool))
			{
				return "boolean";
			}
			if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
			{
				return "smallint";
			}
			if (type == typeof(int) || type == typeof(uint))
			{
				return "integer";
			}
			if (type == typeof(long) || type == typeof(ulong))
			{
				return "bigint";
			}
			if (type == typeof(decimal))
			{
				var declared = (sqlServerType ?? string.Empty).Trim().ToLower();
				return declared.StartsWith("decimal(") || declared.StartsWith("numeric(") ? declared : "numeric(18,4)";
			}
			if (type == typeof(double))
			{
				return "double precision";
			}
			if (type == typeof(float))
			{
				return "real";
			}
			if (type == typeof(DateTime))
			{
				return "timestamp";
			}
			if (type == typeof(DateTimeOffset))
			{
				return "timestamptz";
			}
			if (type == typeof(Guid))
			{
				return "uuid";
			}
			if (type == typeof(byte[]))
			{
				return "bytea";
			}
			if (type == typeof(string))
			{
				var declared = (sqlServerType ?? string.Empty).Trim().ToLower();
				var index = declared.IndexOf('(');
				if (index > 0 && declared.EndsWith(")"))
				{
					var length = declared.Substring(index + 1, declared.Length - index - 2).Trim();
					if (length == "max")
					{
						return "text";
					}
					return "varchar(" + length + ")";
				}
				return "text";
			}
			return sqlServerType;
		}

		/// <summary>
		/// 把实体的 CLR 类型（结合 SqlServer 列类型中声明的长度/精度）转换为 MySQL 列类型。
		/// 布尔统一用 tinyint(1)，避免 MySql.Data 读取 bit 列的兼容问题；布尔列与 0/1 比较在 MySQL 中天然成立。
		/// </summary>
		public static string ToMySqlType(Type clrType, string sqlServerType)
		{
			var type = Nullable.GetUnderlyingType(clrType) ?? clrType;
			if (type == typeof(bool))
			{
				return "tinyint(1)";
			}
			if (type == typeof(byte))
			{
				return "tinyint unsigned";
			}
			if (type == typeof(sbyte))
			{
				return "tinyint";
			}
			if (type == typeof(short))
			{
				return "smallint";
			}
			if (type == typeof(ushort))
			{
				return "smallint unsigned";
			}
			if (type == typeof(int))
			{
				return "int";
			}
			if (type == typeof(uint))
			{
				return "int unsigned";
			}
			if (type == typeof(long))
			{
				return "bigint";
			}
			if (type == typeof(ulong))
			{
				return "bigint unsigned";
			}
			if (type == typeof(decimal))
			{
				var declared = (sqlServerType ?? string.Empty).Trim().ToLower();
				if (declared.StartsWith("decimal(") || declared.StartsWith("numeric("))
				{
					return declared.StartsWith("numeric(") ? "decimal(" + declared.Substring("numeric(".Length) : declared;
				}
				return "decimal(18,4)";
			}
			if (type == typeof(double))
			{
				return "double";
			}
			if (type == typeof(float))
			{
				return "float";
			}
			if (type == typeof(DateTime))
			{
				return "datetime";
			}
			if (type == typeof(DateTimeOffset))
			{
				return "datetime";
			}
			if (type == typeof(Guid))
			{
				return "char(36)";
			}
			if (type == typeof(byte[]))
			{
				return "longblob";
			}
			if (type == typeof(string))
			{
				var declared = (sqlServerType ?? string.Empty).Trim().ToLower();
				var index = declared.IndexOf('(');
				if (index > 0 && declared.EndsWith(")"))
				{
					var length = declared.Substring(index + 1, declared.Length - index - 2).Trim();
					if (length == "max")
					{
						return "longtext";
					}
					// utf8mb4 下单字符占 4 字节，varchar 上限 65535 字节
					if (int.TryParse(length, out var n) && n * 4L <= 65535)
					{
						return "varchar(" + length + ")";
					}
					return "longtext";
				}
				if (declared == "longtext" || declared == "text" || declared == "ntext")
				{
					return "longtext";
				}
				return "longtext";
			}
			return sqlServerType;
		}

		/// <summary>
		/// SQLite 类型声明（SQLite 是动态类型系统，返回与 CLR 类型最匹配的声明即可）
		/// </summary>
		public static string ToSqliteType(Type clrType, string sqlServerType)
		{
			var type = Nullable.GetUnderlyingType(clrType) ?? clrType;
			if (type == typeof(bool)) return "INTEGER";
			if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)) return "INTEGER";
			if (type == typeof(int) || type == typeof(uint)) return "INTEGER";
			if (type == typeof(long) || type == typeof(ulong)) return "INTEGER";
			if (type == typeof(decimal))
			{
				var declared = (sqlServerType ?? string.Empty).Trim().ToLower();
				if (declared.StartsWith("decimal(") || declared.StartsWith("numeric(")) return "NUMERIC";
				return "NUMERIC";
			}
			if (type == typeof(double) || type == typeof(float)) return "REAL";
			if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "TEXT";
			if (type == typeof(Guid)) return "TEXT";
			if (type == typeof(byte[])) return "BLOB";
			if (type == typeof(string)) return "TEXT";
			return "TEXT";
		}
	}
}
