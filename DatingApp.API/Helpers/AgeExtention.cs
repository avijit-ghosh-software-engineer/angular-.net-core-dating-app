using System;


namespace DatingApp.API.Helpers
{
    public static class AgeExtention
    {
        public static int CalculateAge(this DateTime dateTime)
        {
            var age = DateTime.Today.Year - dateTime.Year;
            if (dateTime.AddYears(age) > DateTime.Today)
            {
                age--;
            }
            return age;
        }
    }
}