﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagStatus
    {
        private string code = string.Empty;
        private string description = string.Empty;
        private string historyDescription = string.Empty;
        private string shortDescription = string.Empty;

        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string HistoryDescription
        {
            get
            {
                return historyDescription;
            }
            set
            {
                this.historyDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ShortDescription
        {
            get
            {
                return shortDescription;
            }
            set
            {
                this.shortDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
