using System;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class WorkflowAssociationCreator
    {
        public bool AllowManual { get; set; }
        public string AssociationData { get; set; }
        public bool AutoStartChange { get; set; }
        public bool AutoStartCreate { get; set; }
        public Guid BaseId { get; set; }
        public string Description { get; set; }
        public string HistoryListTitle { get; set; }
        public Guid Id { get; set; }
        public string InstantiationUrl { get; set; }
        public string InternalName { get; set; }
        public bool IsDeclarative { get; set; }
        public string Name { get; set; }
        public string TaskListTitle { get; set; }
    }
}