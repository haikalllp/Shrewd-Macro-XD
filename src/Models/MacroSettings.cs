using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Configuration settings for macro functionality including jitter and recoil reduction
    /// </summary>
    public class MacroSettings : INotifyPropertyChanged
    {
        private int _jitterStrength = 3;
        private int _recoilReductionStrength = 1;
        private bool _jitterEnabled;
        private bool _recoilReductionEnabled;
        private bool _alwaysJitterMode;
        private bool _alwaysRecoilReductionMode;

        /// <summary>
        /// Gets or sets the jitter strength value (1-20)
        /// </summary>
        [Range(1, 20, ErrorMessage = "Jitter strength must be between 1 and 20")]
        public int JitterStrength
        {
            get => _jitterStrength;
            set
            {
                if (_jitterStrength != value)
                {
                    _jitterStrength = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the recoil reduction strength value (1-20)
        /// </summary>
        [Range(1, 20, ErrorMessage = "Recoil reduction strength must be between 1 and 20")]
        public int RecoilReductionStrength
        {
            get => _recoilReductionStrength;
            set
            {
                if (_recoilReductionStrength != value)
                {
                    _recoilReductionStrength = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether jitter is enabled
        /// </summary>
        public bool JitterEnabled
        {
            get => _jitterEnabled;
            set
            {
                if (_jitterEnabled != value)
                {
                    _jitterEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether recoil reduction is enabled
        /// </summary>
        public bool RecoilReductionEnabled
        {
            get => _recoilReductionEnabled;
            set
            {
                if (_recoilReductionEnabled != value)
                {
                    _recoilReductionEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether jitter mode is always enabled
        /// </summary>
        public bool AlwaysJitterMode
        {
            get => _alwaysJitterMode;
            set
            {
                if (_alwaysJitterMode != value)
                {
                    _alwaysJitterMode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether recoil reduction mode is always enabled
        /// </summary>
        public bool AlwaysRecoilReductionMode
        {
            get => _alwaysRecoilReductionMode;
            set
            {
                if (_alwaysRecoilReductionMode != value)
                {
                    _alwaysRecoilReductionMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 