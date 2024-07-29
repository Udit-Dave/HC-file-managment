namespace HCFileCorrection.Entities
{
    public class DTUserMapping
    {
        public int Id { get; set; }
        public int? User_Id { get; set; }
        public int? Country_Id { get; set; }
        public int? Retailer_Id { get; set; }

        public virtual DTCountry Country { get; set; }
        public virtual DTRetailer Retailer { get; set; }
        public virtual UserModel User { get; set; }
    }

}
