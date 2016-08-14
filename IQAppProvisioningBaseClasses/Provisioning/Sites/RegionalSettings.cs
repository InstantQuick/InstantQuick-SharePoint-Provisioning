using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class RegionalSettings
    {
        /// <summary>
        /// Gets or sets the number of days to extend or reduce the current month in Hijri calendars.
        /// </summary>
        public short AdjustHijriDays { get; set; }

        /// <summary>
        /// Gets or sets an alternate calendar type that is used on the server.
        /// </summary>
        public CalendarType AlternateCalendarType { get; set; }

        /// <summary>
        /// Gets or sets the calendar type that is used on the server.
        /// </summary>
        public CalendarType CalendarType { get; set; }

        /// <summary>
        /// Gets or sets the the Collation that is used on the site
        /// </summary>
        public short Collation { get; set; }

        /// <summary>
        /// Gets or sets the first week of the year used in calendars on the server.
        /// </summary>
        public DayOfWeek FirstDayOfWeek { get; set; }

        /// <summary>
        /// The First Week of the Year used in calendars on the server
        /// </summary>
        public short FirstWeekOfYear { get; set; }

        /// <summary>
        /// Gets or sets the locale ID in use on the server
        /// </summary>
        public uint LocaleId { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether to display the week number in day or week views of a calendar
        /// </summary>
        public Boolean ShowWeeks { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether to use a 24-hour time format in representing the hours of the day
        /// </summary>
        public Boolean Time24 { get; set; }

        /// <summary>
        /// The timezone id
        /// </summary>
        public int TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the default hour at which the work day ends on the calendar that is in use on the server.
        /// This is the number of minutes at the top of the hour, e.g. 12:00AM = 0, 11:00PM = 1380
        /// </summary>
        public short WorkDayEndHour { get; set; }

        /// <summary>
        /// Gets or sets a number that represents the work days of Web site calendars.
        /// This is a 7 bit mask where each digit represents a day of the week starting with Sunday (64)
        /// and ending with Saturday (1). Monday-Friday = 0111110 = 62
        /// </summary>
        public short WorkDays { get; set; }

        /// <summary>
        /// Gets or sets the default hour at which the work day starts on the calendar that is in use on the server.
        /// Uses same value system as WorkDayEndHour
        /// </summary>
        public short WorkDayStartHour { get; set; }
    }
}
