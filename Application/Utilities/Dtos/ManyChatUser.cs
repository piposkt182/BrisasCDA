
namespace Application.Utilities.Dtos
{
    public class ManyChatUser
    {
        public string Id { get; set; }
        public string Page_Id { get; set; }
        public List<string> User_Refs { get; set; }
        public string Status { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Profile_Pic { get; set; }
        public string Locale { get; set; }
        public string Language { get; set; }
        public string Timezone { get; set; }
        public string Live_Chat_Url { get; set; }
        public string Last_Input_Text { get; set; }
        public bool Optin_Phone { get; set; }
        public string Phone { get; set; }
        public bool Optin_Email { get; set; }
        public string Email { get; set; }
        public string Subscribed { get; set; }
        public string Last_Interaction { get; set; }
        public string Ig_Last_Interaction { get; set; }
        public string Last_Seen { get; set; }
        public string Ig_Last_Seen { get; set; }
        public bool Is_Followup_Enabled { get; set; }
        public string Ig_Username { get; set; }
        public string Ig_Id { get; set; }
        public string Whatsapp_Phone { get; set; }
        public bool Optin_Whatsapp { get; set; }
        public List<ManyChatCustomField> Custom_Fields { get; set; }
        public List<string> Tags { get; set; }
    }
}
