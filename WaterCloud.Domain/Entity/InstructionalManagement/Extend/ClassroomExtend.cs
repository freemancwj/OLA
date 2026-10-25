using SqlSugar;

namespace WaterCloud.Domain.InstructionalManagement
{
    /// <summary>
    /// 教室扩展（含教学楼名称）
    /// </summary>
    [SugarTable("school_classroom")]
    public class ClassroomExtend : ClassroomEntity
    {
        /// <summary>
        /// 教学楼名称
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string F_BuildingName { get; set; }
    }
}
