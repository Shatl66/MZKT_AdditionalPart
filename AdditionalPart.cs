using e3;
using E3WGM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace E3SetAdditionalPart
{
    [DataContract]
    public class AdditionalPart : Part
    {
        // Я это не использую, но пока оставил
        private double _length = 0;
        internal double length
        {
            get { return _length; }
            set { _length = value; }
        }

        /*        
        private int _lineNumber = 0;
        internal int lineNumber
        {
            get { return _lineNumber; }
            set { _lineNumber = value; }
        }
        */
        public AdditionalPart () : base()
        {
        }

        internal override object[] getDataForRow()
        {
            return new Object[] {oidMaster,
                                    number,
                                    name,
                                    ATR_BOM_RS,
                                    amount};

            /*
            return new Object[] {oidMaster,
                                    number,
                                    name,
                                    ATR_BOM_RS,
                                    ATR_BARCODE,
                                    length,
                                    lineNumber};
            */
        }

        /*
        internal void setLength(string value)
        {
            if (value == "")
            {
                length = 0;
            } else
            {
                length = Double.Parse(value.Replace('.', ','));
            }
            
        }

        internal void setLineNumber(string value)
        {
            if (value == "")
            {
                lineNumber = 0;
            } else
            {
                lineNumber = Int32.Parse(value);
            }
        }
        */
    }
}
