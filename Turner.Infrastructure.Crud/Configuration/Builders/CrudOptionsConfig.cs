namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public class CrudOptionsConfig
    {
        public bool? SuppressCreateActionsInSave { get; set; }

        public bool? SuppressUpdateActionsInSave { get; set; }

        public bool? UseProjection { get; set; }
    }
}
