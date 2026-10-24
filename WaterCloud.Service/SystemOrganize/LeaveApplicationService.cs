using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterCloud.Code;
using WaterCloud.Domain.SystemOrganize;
using WaterCloud.Service.SystemOrganize;
using WaterCloud.Domain.FlowManage;  // 添加流程管理相关命名空间
using WaterCloud.Service.FlowManage;  // 添加流程服务相关命名空间
using WaterCloud.Domain.SystemManage;  // 添加系统管理相关命名空间

namespace WaterCloud.Service.SystemOrganize
{
    public class LeaveApplicationService: BaseService<LeaveApplicationEntity>, IDenpendency
	{
        public FlowinstanceService _flowinstanceService { get; set; }  // 注入流程实例服务
        public LeaveApplicationService(ISqlSugarClient context) : base(context)
       {
        }

    public async Task SubmitForm(LeaveApplicationEntity entity, string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue))
            {
                entity.F_Id = Utils.GuId();
                entity.F_CreatorTime = DateTime.Now;
                entity.F_CreatorUserId = currentuser.UserId;
                entity.F_Status = 0; // 待审批            
                await repository.Insert(entity);
                // 创建流程实例
                await CreateFlowInstance(entity);
            }
            else
            {
                entity.F_Id = keyValue;
                entity.F_LastModifyTime = DateTime.Now;
                entity.F_LastModifyUserId = currentuser.UserId;
                await repository.Update(entity);
            }
        }


        // 创建流程实例
        private async Task CreateFlowInstance(LeaveApplicationEntity leaveEntity)
        {
            // 查找请假流程的模板
            var flowscheme = await repository.Db.Queryable<FlowschemeEntity>()
                .Where(s => s.F_SchemeCode == "LeaveApplication" && s.F_DeleteMark == false)
                .FirstAsync();

            if (flowscheme == null)
            {
                throw new Exception("未找到请假流程模板，请先在系统中设计请假流程");
            }

            // 创建流程实例
            var flowinstance = new FlowinstanceEntity
            {
                F_Id = Utils.GuId(),
                F_SchemeId = flowscheme.F_Id,
                F_SchemeType = flowscheme.F_SchemeCode, // 修改为使用F_SchemeType
                F_Description = flowscheme.F_SchemeName, // 修改为使用F_Description
                F_SchemeContent = flowscheme.F_SchemeContent,
                F_FrmId = flowscheme.F_FrmId,
                F_FrmType = flowscheme.F_FrmType,
                F_Code = leaveEntity.F_Id,  // 关联请假申请ID
                F_CustomName = $"{leaveEntity.F_ApplicantName}的请假申请",
                F_CreatorUserId = currentuser.UserId,
                F_CreatorTime = DateTime.Now,
            };

            // 构建表单数据
            var formData = new Dictionary<string, string>
            {
                { "申请人", currentuser.UserId },
                { "所属部门", currentuser.OrganizeId },
                { "请假类型", leaveEntity.F_LeaveType.ToString() },
                { "开始时间", leaveEntity.F_StartTime.ToString() },
                { "结束时间", leaveEntity.F_EndTime.ToString() },
                { "请假天数", leaveEntity.F_LeaveDays.ToString() },
                { "请假原因", leaveEntity.F_Reason }
            };

            flowinstance.F_FrmData = formData.ToJson();

            // 调用流程服务创建实例
            await _flowinstanceService.CreateInstance(flowinstance);


            // 更新请假申请的流程实例ID
            leaveEntity.F_FlowInstanceId = flowinstance.F_Id;
            await repository.Update(leaveEntity);
        }

        //获取直系领导
        private async Task<string> GetDirectLeader()
        {
            // 获取当前用户ID
            string currentUserId = currentuser.UserId;


            // 查询当前用户信息 - 使用ToListAsync然后取第一个元素
            UserEntity currentUser = (await repository.Db.Queryable<UserEntity>()
                .Where(u => u.F_Id == currentUserId)
                .ToListAsync()).FirstOrDefault();

            if (currentUser != null && !string.IsNullOrEmpty(currentUser.F_ManagerId))
            {
                // 返回直系领导ID
                return currentUser.F_ManagerId;
            }

            // 如果没有找到直系领导，返回空字符串
            return "";
        }

        private async Task<string> GetGeneralManager()
        {

            // 查找F_IsBoss为true的用户，假设这是总经理
            UserEntity generalManager = (await repository.Db.Queryable<UserEntity>()
                .Where(u => u.F_IsBoss == true)
                .ToListAsync()).FirstOrDefault();

            if (generalManager != null)
            {
                return generalManager.F_Id;
            }

            // 如果没有找到总经理，返回空字符串
            return "";
        }
        //获取请假申请列表（带分页和关键词搜索）
        public async Task<List<LeaveApplicationEntity>> GetList(SoulPage<LeaveApplicationEntity> pagination, string keyword)
        {
            // 设置查询条件
            var query = repository.Db.Queryable<LeaveApplicationEntity, UserEntity>((la, u) => new object[]
            {
                JoinType.Left, la.F_EmployeeId == u.F_Id
            })
            .Select((la, u) => new LeaveApplicationEntity
            {
                F_Id = la.F_Id,
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
                F_LastModifyTime = la.F_LastModifyTime,
                F_LastModifyUserId = la.F_LastModifyUserId,
                // 添加一个扩展属性来存储申请人名称
                F_ApplicantName = u.F_RealName
            });

            // 如果有关键词，添加搜索条件
            if (!string.IsNullOrEmpty(keyword))
            {              
                query.Where(item => item.F_Reason.Contains(keyword) ||
                               item.F_ApplicantName.Contains(keyword) ||
                               item.F_LeaveType.ToString().Contains(keyword));
            }

            // 应用分页排序
            if (!string.IsNullOrEmpty(pagination.field) && !string.IsNullOrEmpty(pagination.order))
            {
                // 根据字段和排序方向进行排序
                query = query.OrderBy($"{pagination.field} {pagination.order}");
            }
            else
            {
                // 默认排序
                query = query.OrderBy("F_CreatorTime desc");
            }

            // 获取总数
            pagination.count = await query.CountAsync();

            // 获取分页数据
            var data = await query.ToPageListAsync(pagination.page, pagination.rows);

            return data;
        }

        //获取单个请假申请表单数据
        public async Task<LeaveApplicationEntity> GetForm(string keyValue)
        {
            return (await repository.Db.Queryable<LeaveApplicationEntity>().In(keyValue).ToListAsync()).FirstOrDefault();
        }

        //删除请假申请
        public async Task DeleteForm(string keyValue)
        {
            // 先根据keyValue获取实体对象
            var entity = await GetForm(keyValue);
            if (entity != null)
            {
                // 然后删除该实体
                await repository.Delete(entity);
            }
        }
    }
}