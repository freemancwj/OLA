using SqlSugar;
using System;
using System.Linq;
using WaterCloud.Code;

namespace WaterCloud.DataBase
{
	public class DBContexHelper
	{
		private static string defaultDbType = GlobalContext.SystemConfig.DBProvider;
		private static string defaultDbConnectionString = GlobalContext.SystemConfig.DBConnectionString;

		public static ConnectionConfig Contex(string ConnectStr = "", string providerName = "")
		{
			ConnectStr = string.IsNullOrEmpty(ConnectStr) ? defaultDbConnectionString : ConnectStr;
			providerName = string.IsNullOrEmpty(providerName) ? defaultDbType : providerName;
			var dbType = ParseDbType(providerName);
			var config = new ConnectionConfig()
			{
				DbType = dbType,
				InitKeyType = InitKeyType.Attribute,
				IsAutoCloseConnection = true,
				ConnectionString = ConnectStr,
				MoreSettings = new ConnMoreSettings()
				{
					IsAutoRemoveDataCache = true//自动清理缓存
				},
				ConfigureExternalServices = new ConfigureExternalServices()
				{
					DataInfoCacheService = new SqlSugarCache(), //配置我们创建的缓存类
					EntityService = (property, column) =>
					{
						var attributes = property.GetCustomAttributes(true);//get all attributes
						if (dbType == DbType.SqlServer && attributes.Any(it => it is SugarColumn) && column.DataType == "longtext")
						{
							column.DataType = "nvarchar(4000)";
						}
						if (dbType == DbType.PostgreSQL)
						{
							//PostgreSQL：按 CLR 类型把实体的 SqlServer 列类型转换为 PostgreSQL 类型
							column.DataType = SqlDialect.ToPostgreSqlType(property.PropertyType, column.DataType);
						}
						if (dbType == DbType.Sqlite)
						{
							//SQLite：按 CLR 类型把实体的 SqlServer 列类型转换为 SQLite 类型声明
							column.DataType = SqlDialect.ToSqliteType(property.PropertyType, column.DataType);
						}
						if (dbType == DbType.MySql)
						{
							//MySQL：按 CLR 类型把实体的 SqlServer 列类型转换为 MySQL 类型
							column.DataType = SqlDialect.ToMySqlType(property.PropertyType, column.DataType);
							//MySQL 不允许 TEXT/BLOB 主键：未显式声明列类型的字符串主键兜底为 varchar(50)
							var isPk = column.IsPrimarykey || attributes.Any(a => a is SugarColumn && ((SugarColumn)a).IsPrimaryKey);
							if (isPk && column.DataType == "longtext")
							{
								column.DataType = "varchar(50)";
							}
						}
					}
				}
			};
			if (dbType == DbType.PostgreSQL)
			{
				//PostgreSQL 统一使用小写标识符，保证原生 SQL 中的列名（不区分大小写）可正常匹配
				config.MoreSettings.PgSqlIsAutoToLower = true;
			}
			return config;
		}

		/// <summary>
		/// 解析数据库类型，兼容 PostgreSQL 的常见别名
		/// </summary>
		public static DbType ParseDbType(string providerName)
		{
			if (string.IsNullOrWhiteSpace(providerName))
			{
				return DbType.SqlServer;
			}
			var name = providerName.Trim();
			if (name.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("PgSql", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("Pgsql", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("Npgsql", StringComparison.OrdinalIgnoreCase))
			{
				return DbType.PostgreSQL;
			}
			if (name.Equals("Sqlite", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("SQLite", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase))
			{
				return DbType.Sqlite;
			}
			if (name.Equals("MySql", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("MySQL", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("MySql.Data", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("MySqlConnector", StringComparison.OrdinalIgnoreCase))
			{
				return DbType.MySql;
			}
			if (Enum.TryParse<DbType>(name, true, out var parsed))
			{
				return parsed;
			}
			throw new ArgumentException("不支持的数据库类型：" + providerName);
		}
	}
}
