using EventManagerWebRazorPage.Models;

namespace EventManagerWebRazorPage.Services.Other
{
    public static class ValidateDateTime
    {
        public static bool ValidDateTime(this DateTime inputDate, DateTime registerStartDate, DateTime registerEndDate)
        {
            return (inputDate < registerEndDate.Date.AddDays(1) && inputDate >= registerStartDate);
        }
        public static bool ValidDateTime(this DateTime inputDate, DateTime EventDate)
        {
            return (inputDate < EventDate.Date.AddDays(1) && inputDate >= EventDate);
        }
        /// <summary>
        /// Returns true if the inputDate is in between the RegisterStartDate and RegisterEndDate, or in the EventTimeline
        /// </summary>
        /// <param name="inputDate"></param>
        /// <param name="eventDetail"></param>
        /// <param name="UseRegisterTimeline"></param>
        /// <returns></returns>
        public static bool ValidDateTime(this DateTime inputDate, EventDetail eventDetail, bool UseRegisterTimeline = true)
        {
            if(UseRegisterTimeline)
                return ValidDateTime(inputDate, eventDetail.RegistrationStartDate, eventDetail.RegistrationEndDate);
            else
                return ValidDateTime(inputDate, eventDetail.EventTime);
        }

        public static bool CompareDate(DateTime firstDate, DateTime secondDate, string expression = "=")
        {
            return expression switch
            {
                "<" => (firstDate.Date.CompareTo(secondDate.Date) < 0),
                "<=" => (firstDate.Date.CompareTo(secondDate.Date) <= 0),
                ">" => (firstDate.Date.CompareTo(secondDate.Date) > 0),
                ">=" => (firstDate.Date.CompareTo(secondDate.Date) >= 0),
                _ => (firstDate.Date.CompareTo(secondDate.Date) == 0),
            };
        }
    }
}
