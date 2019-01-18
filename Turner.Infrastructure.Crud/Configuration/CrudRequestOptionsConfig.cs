namespace Turner.Infrastructure.Crud.Configuration
{
    public class CrudRequestOptionsConfig
    {
        public bool? SuppressCreateActionsInSave { get; set; }

        public bool? SuppressUpdateActionsInSave { get; set; }

        public bool? UseProjection { get; set; }
    }
}
