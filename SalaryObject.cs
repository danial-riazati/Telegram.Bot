using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Main
{
    public class SalaryObject
    {
        public long chatId { get; set; }
        public int month { get; set; }
        private int hoursOfMonth { get; set; }
        public long salary { get; set; }

        public long salaryInHour { get; set; }
        public int Insured { get; set; }
        public bool isInsuredPlus { get; set; }
        public int hours { get; set; }
        public SalaryObject(long chatId)
        {
            this.chatId = chatId;
            hours = 0;
            Insured = 0;
            isInsuredPlus = false;
            salary = 0;
            month = 1;
            salaryInHour = 38000;
        }
        public override string ToString()
        {
            string t = isInsuredPlus ? "بله" : "خیر";
            var nakhales = hours * salaryInHour;
            CalculateSalary();
            string data = $"ماه انتخابی: {month}\nساعات کاری مصوب: {hoursOfMonth}\nساعات بیمه تامین اجتماعی رد شده: {Insured}\nبیمه تکمیلی: {t}\nساعات کاری شما: {hours}\nدریافتی شما به ازای هر ساعت: {salaryInHour}\nحقوق ناخالص شما: {nakhales}\n------------------------\nدریافتی: {salary}\n";
            return data;
        }

        private void CalculateSalary()
        {
            int mhours;
            switch (month)
            {
                case 1:
                    hoursOfMonth = 154;
                    calc(hoursOfMonth);
                    return;
                case 2:
                    hoursOfMonth = 169;
                    calc(hoursOfMonth);
                    return;
                case 3:
                    hoursOfMonth = 183;
                    calc(hoursOfMonth);
                    return;
                case 4:
                    hoursOfMonth = 191;
                    calc(hoursOfMonth);
                    return;
                case 5:
                    hoursOfMonth = 169;
                    calc(hoursOfMonth);
                    return;
                case 6:
                    hoursOfMonth = 198;
                    calc(hoursOfMonth);
                    return;
                case 7:
                    hoursOfMonth = 161;
                    calc(hoursOfMonth);
                    return;
                case 8:
                    hoursOfMonth = 183;
                    calc(hoursOfMonth);
                    return;
                case 9:
                    hoursOfMonth = 191;
                    calc(hoursOfMonth);
                    return;
                case 10:
                    hoursOfMonth = 183;
                    calc(hoursOfMonth);
                    return;
                case 11:
                    hoursOfMonth = 176;
                    calc(hoursOfMonth);
                    return;
                case 12:
                    hoursOfMonth = 169;
                    calc(hoursOfMonth);
                    return;
            }
        }
        private void calc(int mhours)
        {
            long fee;
            Insured = (Insured >= mhours) ? mhours : Insured;
            if (Insured < hours)
                fee = (7 * Insured * salaryInHour / 100);
            else
                fee = (30 * (Insured - hours) * salaryInHour / 100) + (7 * (hours) * salaryInHour / 100);
            salary = (isInsuredPlus ? salaryInHour * hours - fee - 150000 : salaryInHour * hours - fee);
        }

    }
    /*    public Map<String,String> Month
        {
            nothing,
            farvardin = "dasdas",
            ordibehesht,
            khordad,
            tir,
            mordad,
            shahrivar,
            mehr,
            aban,
            azar,
            dey,
            bahman,
            esfand
        }*/

}
