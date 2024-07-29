using OpenQA.Selenium;

namespace HCFileCorrection.Entities
{
    public class DTUserSessionModel
    {
        public int Session_Id { get; set; }
        public int UserId { get; set; }
        public Guid SessionGuid { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsActive { get; set; }
        public DateTime SessionCreatedDateTime { get; set; }
        public DateTime SessionEndDateTime { get; set; }

        public UserModel User { get; set; }

    }
}
