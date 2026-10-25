using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Domain.SystemManage;
using WaterCloud.Domain.FlowManage;
using WaterCloud.Service.FlowManage;

namespace WaterCloud.Service.SystemOrganize
{
	/// <summary>
	/// 请假申请服务（教师/学生通用，驱动工作流审批）
	/// </summary>
	public class LeaveApplicationService : BaseService<LeaveApplicationEntity>, IDenpendency
	{
		public FlowinstanceService _flowinstanceService { get; set; }

		/// <summary>
		/// 学生角色Id（R_03），用于区分学生请假与教师请假
		/// </summary>
		private const string StudentRoleId = "08da9c6b-f455-46c0-8ffe-81edad14c7be";

		public LeaveApplicationService(ISqlSugarClient context) : base(context)
		{
		}

		#region 提交数据

		/// <summary>
		/// 提交请假申请（新增发起流程；驳回/撤回后修改则重提原流程实例）
		/// </summary>
		public async Task SubmitForm(LeaveApplicationEntity entity, string keyValue)
		{
			if (entity.F_EndTime <= entity.F_StartTime)
			{
				throw new Exception("结束时间必须大于开始时间");
			}
			if (entity.F_LeaveDays <= 0)
			{
				throw new Exception("请假天数必须大于0");
			}
			if (string.IsNullOrEmpty(keyValue))
			{
				entity.F_Id = Utils.GuId();
				entity.F_CreatorTime = DateTime.Now;
				entity.F_CreatorUserId = currentuser.UserId;
				entity.F_EmployeeId = currentuser.UserId;
				entity.F_ApplicantType = IsStudent() ? 2 : 1;
				entity.F_Status = 0; //审批中
				await repository.Insert(entity);
				await CreateFlowInstance(entity);
			}
			else
			{
				var leave = await GetForm(keyValue);
				if (leave == null)
				{
					throw new Exception("请假申请不存在");
				}
				if (leave.F_EmployeeId != currentuser.UserId && !currentuser.IsSuperAdmin)
				{
					throw new Exception("只能修改本人的请假申请");
				}
				if (leave.F_Status != 3 && leave.F_Status != 4)
				{
					throw new Exception("仅被驳回或已撤回的申请可以修改后重新提交");
				}
				leave.F_LeaveType = entity.F_LeaveType;
				leave.F_StartTime = entity.F_StartTime;
				leave.F_EndTime = entity.F_EndTime;
				leave.F_LeaveDays = entity.F_LeaveDays;
				leave.F_Reason = entity.F_Reason;
				leave.F_Status = 0;
				leave.F_LastModifyTime = DateTime.Now;
				leave.F_LastModifyUserId = currentuser.UserId;
				await repository.Db.Updateable(leave).ExecuteCommandAsync();
				await ReCreateFlowInstance(leave);
			}
		}

		/// <summary>
		/// 撤回请假申请（审批中才能撤回）
		/// </summary>
		public async Task CancleForm(string keyValue)
		{
			var leave = await GetForm(keyValue);
			if (leave == null)
			{
				throw new Exception("请假申请不存在");
			}
			if (leave.F_EmployeeId != currentuser.UserId && !currentuser.IsSuperAdmin)
			{
				throw new Exception("只能撤回本人的请假申请");
			}
			if (leave.F_Status != 0)
			{
				throw new Exception("申请已处理，无法撤回");
			}
			if (!string.IsNullOrEmpty(leave.F_FlowInstanceId))
			{
				//撤回流程实例，钩子会将请假状态同步为已撤回
				await _flowinstanceService.CancleForm(leave.F_FlowInstanceId);
			}
			else
			{
				await repository.Db.Updateable<LeaveApplicationEntity>()
					.SetColumns(a => a.F_Status == 4)
					.Where(a => a.F_Id == leave.F_Id).ExecuteCommandAsync();
			}
		}

		/// <summary>
		/// 删除请假申请（审批中不可删除）
		/// </summary>
		public async Task DeleteForm(string keyValue)
		{
			var leave = await GetForm(keyValue);
			if (leave == null)
			{
				return;
			}
			if (leave.F_Status == 0)
			{
				throw new Exception("审批中的申请不可删除，请先撤回");
			}
			await repository.Delete(leave);
		}

		#endregion 提交数据

		#region 流程实例

		/// <summary>
		/// 当前登录人是否学生
		/// </summary>
		private bool IsStudent()
		{
			return !string.IsNullOrEmpty(currentuser.RoleId) && currentuser.RoleId.Contains(StudentRoleId);
		}

		/// <summary>
		/// 构建流程表单数据（键为中文，与流程表单字段及分支条件对应）
		/// </summary>
		private async Task<Dictionary<string, string>> BuildFrmData(LeaveApplicationEntity leave)
		{
			var user = await repository.Db.Queryable<UserEntity>().InSingleAsync(leave.F_EmployeeId);
			string deptName = "";
			if (user != null && !string.IsNullOrEmpty(user.F_OrganizeId))
			{
				var deptId = user.F_OrganizeId.Split(',')[0];
				deptName = await repository.Db.Queryable<OrganizeEntity>().Where(a => a.F_Id == deptId).Select(a => a.F_FullName).FirstAsync();
			}
			return new Dictionary<string, string>
			{
				{ "申请人", user != null ? user.F_RealName : currentuser.UserName },
				{ "所属部门", deptName ?? "" },
				{ "申请类型", leave.F_ApplicantType == 2 ? "学生请假" : "教师请假" },
				{ "请假类型", leave.F_LeaveType.ToString() },
				{ "开始时间", leave.F_StartTime.ToString("yyyy-MM-dd HH:mm") },
				{ "结束时间", leave.F_EndTime.ToString("yyyy-MM-dd HH:mm") },
				{ "请假天数", leave.F_LeaveDays.ToString() },
				{ "请假原因", leave.F_Reason ?? "" }
			};
		}

		/// <summary>
		/// 按申请人类型获取流程模板
		/// </summary>
		private async Task<FlowschemeEntity> GetLeaveScheme(int applicantType)
		{
			string schemeCode = applicantType == 2 ? "StudentLeave" : "TeacherLeave";
			var flowscheme = await repository.Db.Queryable<FlowschemeEntity>()
				.Where(s => s.F_SchemeCode == schemeCode && s.F_DeleteMark == false)
				.FirstAsync();
			if (flowscheme == null)
			{
				throw new Exception("未找到请假流程模板(" + schemeCode + ")，请先在流程设计中配置");
			}
			return flowscheme;
		}

		/// <summary>
		/// 创建流程实例并回写流程信息
		/// </summary>
		private async Task CreateFlowInstance(LeaveApplicationEntity leaveEntity)
		{
			var flowscheme = await GetLeaveScheme(leaveEntity.F_ApplicantType);
			var frmData = await BuildFrmData(leaveEntity);
			var flowinstance = new FlowinstanceEntity
			{
				F_Id = Utils.GuId(),
				F_SchemeId = flowscheme.F_Id,
				F_SchemeType = flowscheme.F_SchemeCode,
				F_Description = flowscheme.F_SchemeName,
				F_Code = leaveEntity.F_Id,
				F_CustomName = frmData["申请人"] + "的请假申请",
				F_FrmData = frmData.ToJson(),
				F_CreatorUserId = currentuser.UserId,
				F_CreatorTime = DateTime.Now,
			};
			await _flowinstanceService.CreateInstance(flowinstance);
			await repository.Db.Updateable<LeaveApplicationEntity>()
				.SetColumns(a => new LeaveApplicationEntity { F_FlowInstanceId = flowinstance.F_Id })
				.Where(a => a.F_Id == leaveEntity.F_Id).ExecuteCommandAsync();
			await SyncApprover(flowinstance.F_Id, leaveEntity.F_Id);
		}

		/// <summary>
		/// 驳回/撤回后重新提交，重置原流程实例
		/// </summary>
		private async Task ReCreateFlowInstance(LeaveApplicationEntity leaveEntity)
		{
			var flowscheme = await GetLeaveScheme(leaveEntity.F_ApplicantType);
			var frmData = await BuildFrmData(leaveEntity);
			var flowinstance = new FlowinstanceEntity
			{
				F_Id = leaveEntity.F_FlowInstanceId,
				F_SchemeId = flowscheme.F_Id,
				F_SchemeType = flowscheme.F_SchemeCode,
				F_Description = flowscheme.F_SchemeName,
				F_Code = leaveEntity.F_Id,
				F_CustomName = frmData["申请人"] + "的请假申请",
				F_FrmData = frmData.ToJson(),
			};
			await _flowinstanceService.UpdateInstance(flowinstance);
			await SyncApprover(flowinstance.F_Id, leaveEntity.F_Id);
		}

		/// <summary>
		/// 同步当前审批人姓名到请假单
		/// </summary>
		private async Task SyncApprover(string flowInstanceId, string leaveId)
		{
			var instance = await repository.Db.Queryable<FlowinstanceEntity>().InSingleAsync(flowInstanceId);
			string approver = "";
			if (instance != null && !string.IsNullOrEmpty(instance.F_MakerList) && instance.F_MakerList != "1")
			{
				var ids = instance.F_MakerList.Split(',').ToList();
				approver = string.Join(",", await repository.Db.Queryable<UserEntity>().Where(a => ids.Contains(a.F_Id)).Select(a => a.F_RealName).ToListAsync());
			}
			await repository.Db.Updateable<LeaveApplicationEntity>()
				.SetColumns(a => new LeaveApplicationEntity { F_CurrentApprover = approver })
				.Where(a => a.F_Id == leaveId).ExecuteCommandAsync();
		}

		#endregion 流程实例

		#region 获取数据

		/// <summary>
		/// 请假申请列表（普通用户仅看本人申请，管理员可看全部）
		/// </summary>
		public async Task<List<LeaveApplicationEntity>> GetList(SoulPage<LeaveApplicationEntity> pagination, string keyword, string leaveType, string status)
		{
			var queryable = repository.Db.Queryable<LeaveApplicationEntity, UserEntity>((la, u) => new object[]
			{
				JoinType.Left, la.F_EmployeeId == u.F_Id
			});
			//普通用户仅看本人申请，管理员可看全部
			if (!currentuser.IsSuperAdmin && !currentuser.IsAdmin)
			{
				queryable = queryable.Where((la, u) => la.F_EmployeeId == currentuser.UserId);
			}
			if (!string.IsNullOrEmpty(keyword))
			{
				queryable = queryable.Where((la, u) => la.F_Reason.Contains(keyword) || u.F_RealName.Contains(keyword));
			}
			if (!string.IsNullOrEmpty(leaveType))
			{
				var type = int.Parse(leaveType);
				queryable = queryable.Where((la, u) => la.F_LeaveType == type);
			}
			if (!string.IsNullOrEmpty(status))
			{
				var s = int.Parse(status);
				queryable = queryable.Where((la, u) => la.F_Status == s);
			}
			//联表查询下字符串排序会产生列名歧义，固定按请假单创建时间倒序
			queryable = queryable.OrderBy((la, u) => la.F_CreatorTime, OrderByType.Desc);
			var query = queryable.Select((la, u) => new LeaveApplicationEntity
			{
				F_Id = la.F_Id,
				F_FlowInstanceId = la.F_FlowInstanceId,
				F_ApplicantType = la.F_ApplicantType,
				F_EmployeeId = la.F_EmployeeId,
				F_LeaveType = la.F_LeaveType,
				F_StartTime = la.F_StartTime,
				F_EndTime = la.F_EndTime,
				F_LeaveDays = la.F_LeaveDays,
				F_Reason = la.F_Reason,
				F_Status = la.F_Status,
				F_CurrentApprover = la.F_CurrentApprover,
				F_CreatorTime = la.F_CreatorTime,
				F_CreatorUserId = la.F_CreatorUserId,
				F_ApplicantName = u.F_RealName
				});
			pagination.count = await query.CountAsync();
			return await query.ToPageListAsync(pagination.page, pagination.rows);
		}

		/// <summary>
		/// 获取单个请假申请
		/// </summary>
		public async Task<LeaveApplicationEntity> GetForm(string keyValue)
		{
			return await repository.FindEntity(keyValue);
		}

		#endregion 获取数据
	}
}
