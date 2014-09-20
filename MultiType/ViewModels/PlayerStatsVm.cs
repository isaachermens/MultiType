using System;
using System.ComponentModel;
using System.Windows.Controls;
using MultiType.Models;
using MultiType.SocketsAPI;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
    public class PlayerStatsVm : BaseVm
	{
        // todo set all of these things
        public int CompletionPercentage { get; set; }
        
		public int CharactersTyped { get; set; }
        public int Accuracy { get; set; }
        public int ErrorCount { get; set; }
        public string TimeElapsed { get; set; }
        public int WPM { get; set; }

        public PlayerStatsVm()
        {
            CharactersTyped = 0;
            Accuracy = 0;
            // todo include lesson length in completion percentage
            CompletionPercentage = 0;
            ErrorCount = 0;
            WPM = 0;
		}

        public void UpdateStats(UserStatistics stats)
        {
            CompletionPercentage = stats.CompletionPercentage;
            CharactersTyped = stats.CharactersTyped;
            Accuracy = stats.Accuracy;
            ErrorCount = stats.Errors;
            WPM = stats.WPM;
        }
	}
}
