using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;

namespace InformatorGorlicki
{
    public class News
    {

        private dynamic Jobj { get; set; }
        public int CountGroups { get; set; }
        public int CountMessages { get; set; }
        public int GID { get; set; }
        public int NID { get; set; }
        public string Link { get; set; }

        public List<string> LastDate = new List<string>();
        public List<int> New = new List<int>();

        public void LoadHosts(string str, ComboBox cb)
        {
            try
            {
                dynamic hosts = JsonConvert.DeserializeObject<dynamic>(str);
                cb.Items.Clear();

                foreach (string host in hosts)
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = host
                    };
                    _ = cb.Items.Add(item);
                }
            }
            catch (Exception)
            {

            }
        }

        public void Get(ref string str)
        {


            string date = string.Empty;
            string text = string.Empty;

            try
            {
                var rows = Jobj[GID];
                CountMessages = Count(rows);
                var field = rows[NID];
                date = field["date"];
                Link = field["link"];
                text = field["text"];
                str = date + " " + text.Replace("&quot;", "\"");
            }
            catch (Exception) { }
        }

        public void Set(string str)
        {
            Jobj = JsonConvert.DeserializeObject<dynamic>(str);
            CountGroups = Count(Jobj);
            GID = 0;
            NID = 0;
        }

        public bool IsNew()
        {
            try
            {
                if (LastDate.Count < CountGroups)
                {
                    for (var i = 0; i < CountGroups; i++)
                    {
                        LastDate.Add(string.Empty);
                    }
                }

                string date = Jobj[GID][0].date;
                string last_date = LastDate[GID];

                if (last_date == "")
                {
                    LastDate[GID] = date;
                }
                else if (last_date.Equals(date) == false)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public int CountNewMessages()
        {
            int cnt = 0;

            for (int i = 0; i < CountGroups; i++)
            {
                string date = Jobj[i][0].date;

                if (LastDate[i].Equals(date) == false)
                {
                    cnt++;
                }
            }

            return cnt;
        }

        public void SetAsRead()
        {
            string date = Jobj[GID][0].date;
            LastDate[GID] = date;
        }

        private int Count(dynamic obj)
        {
            int cnt = 0;

            foreach (var r in obj)
            {
                cnt++;
            }

            return cnt;
        }
    }
}
