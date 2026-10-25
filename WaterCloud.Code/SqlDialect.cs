using System;
using System.Text.RegularExpressions;

namespace WaterCloud.Code
{
	/// <summary>
	/// 数据库方言适配。
	/// 系统内的原生 SQL 统一按 SqlServer 语法书写，运行时根据当前数据库类型转换为目标数据库语法，
	/// 目前主要解决 PostgreSQL 与 SqlServer 的差异（ISNULL、COLLATE、CONVERT、字符串拼接、布尔值等）。
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
		/// 当前是否是 PostgreSQL 数据库
		/// </summary>
		public static bool IsPostgreSql
		{
			get
			{
				var provider = GlobalContext.SystemConfig == null ? null : GlobalContext.SystemConfig.DBProvider;
				return !string.IsNullOrEmpty(provider) && provider.IndexOf("postgre", StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		/// <summary>
		/// 布尔常量（用于拼接原生 SQL 的默认值）
		/// </summary>
		public static string Bool(bool value)
		{
			if (IsPostgreSql)
			{
				return value ? "TRUE" : "FALSE";
			}
			return value ? "1" : "0";
		}

		/// <summary>
		/// SELECT 后的 TOP 关键字（PostgreSQL 使用 LIMIT，由 <see cref="Limit"/> 补在语句末尾）
		/// </summary>
		public static string Top(int count)
		{
			return IsPostgreSql ? string.Empty : "TOP " + count + " ";
		}

		/// <summary>
		/// 语句末尾的 LIMIT 关键字（PostgreSQL）
		/// </summary>
		public static string Limit(int count)
		{
			return IsPostgreSql ? " LIMIT " + count : string.Empty;
		}

		/// <summary>
		/// 把 SqlServer 语法的原生 SQL 转换为当前数据库语法
		/// </summary>
		public static string Trans(string sql)
		{
			if (string.IsNullOrEmpty(sql) || !IsPostgreSql)
			{
				return sql;
			}
			var result = sql;
			// 排序规则（PostgreSQL 不支持）
			result = Regex.Replace(result, @"\s+COLLATE\s+[\w]+", string.Empty, RegexOptions.IgnoreCase);
			// Unicode 字符串前缀
			result = Regex.Replace(result, @"(?<![\w'])N'", "'", RegexOptions.IgnoreCase);
			// CONVERT(bit,0/1)
			result = Regex.Replace(result, @"CONVERT\s*\(\s*bit\s*,\s*1\s*\)", "TRUE", RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"CONVERT\s*\(\s*bit\s*,\s*0\s*\)", "FALSE", RegexOptions.IgnoreCase);
			// CONVERT(varchar(n), expr, 108)：时间列本身已是字符串，直接取原值
			result = Regex.Replace(result, @"CONVERT\s*\(\s*varchar\s*\(\s*\d+\s*\)\s*,\s*([\w\.]+)\s*,\s*108\s*\)", "$1", RegexOptions.IgnoreCase);
			// CONVERT(varchar(n), expr, 23)：日期格式化
			result = Regex.Replace(result, @"CONVERT\s*\(\s*varchar\s*\(\s*\d+\s*\)\s*,\s*([\w\.]+)\s*,\s*23\s*\)", "to_char($1,'YYYY-MM-DD')", RegexOptions.IgnoreCase);
			// 字符串拼接：+ 换成 ||
			result = Regex.Replace(result, @"'\s*\+\s*", "' || ");
			result = Regex.Replace(result, @"\s*\+\s*'", " || '");
			// ISNULL 换成 COALESCE
			result = Regex.Replace(result, @"\bISNULL\s*\(", "COALESCE(", RegexOptions.IgnoreCase);
			// 布尔列默认值：COALESCE(布尔列,0/1)
			result = Regex.Replace(result, @"(COALESCE\s*\(\s*(?:[\w]+\.)?(?:" + BoolPattern + @")\s*,\s*)0(\s*\))",
				"$1FALSE$2", RegexOptions.IgnoreCase);
			result = Regex.Replace(result, @"(COALESCE\s*\(\s*(?:[\w]+\.)?(?:" + BoolPattern + @")\s*,\s*)1(\s*\))",
				"$1TRUE$2", RegexOptions.IgnoreCase);
			// 布尔表达式与 0/1 的比较：COALESCE(...) = 0
			result = Regex.Replace(result, @"(COALESCE\s*\(\s*(?:[\w]+\.)?(?:" + BoolPattern + @")\s*,\s*(?:TRUE|FALSE)\s*\))\s*(=|<>|!=)\s*([01])\b",
				MatchBoolCompare, RegexOptions.IgnoreCase);
			// 布尔列与 0/1 的比较：a.F_DeleteMark = 0
			result = Regex.Replace(result, @"((?:[\w]+\.)?(?:" + BoolPattern + @"))\s*(=|<>|!=)\s*([01])\b",
				MatchBoolCompare, RegexOptions.IgnoreCase);
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
	}
}
