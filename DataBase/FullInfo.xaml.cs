using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DataBase
{
    public partial class FullInfo : Window
    {
        int rowNo = 0;
        int pageNo = 0;
        internal Threat threat;

        public FullInfo(int rowNumber,
                        int pageNumper,
                        string dif)
        {
            InitializeComponent();
            rowNo = rowNumber;
            pageNo = pageNumper;
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            CloseButton.Click += (s, e) => Close();
            ShowFullinfo(dif);
        }

        private void ShowFullinfo(string dif)
        {
            if (threat == null)
            {
                threat = Sheet.threatDict.Values.ToList()[rowNo + 20 * pageNo];
            }    
            Header.Text = threat.Name + dif;
            Description.Text = "Описание: " + threat.Description;
            Source.Text = "Источник угрозы (характеристика и потенциал нарушителя): " + threat.Source;
            Aim.Text = "Объект воздействия: " + threat.Aim;
            ConfidentialityBreak.Text = "Нарушение конфиденциальности: " + threat.ConfidentialityBreak;
            IntegrityBreak.Text = "Нарушение целостности: " + threat.IntegrityBreak;
            AvailabilityBreak.Text = "Нарушение доступности: " + threat.AvailabilityBreak;
            AppearDate.Text = "Дата включения угрозы в БнД УБИ: " + threat.AppearDate.ToString("dd.MM.yyyy");
            LastUpdateDay.Text = "Дата последнего изменения данных: " + threat.LastUpdateDate.ToString("dd.MM.yyyy");
        }


    }
}
