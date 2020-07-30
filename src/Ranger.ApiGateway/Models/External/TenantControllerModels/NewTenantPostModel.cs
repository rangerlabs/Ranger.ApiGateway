namespace Ranger.ApiGateway
{
    public class NewTenantPostModel
    {
        public OrganizationFormPostModel OrganizationForm { get; set; }
        public UserForm UserForm { get; set; }
    }
}