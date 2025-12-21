using Aspose.Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.RPT.Enums
{
    public enum AlignmentEnum
    {
        None = -1,
        Left = ParagraphAlignment.Left,
        Center = ParagraphAlignment.Center,
        Right = ParagraphAlignment.Right,
        Justify = ParagraphAlignment.Justify,
        Distributed = ParagraphAlignment.Distributed,
        ArabicMediumKashida = ParagraphAlignment.ArabicMediumKashida,
        ArabicHighKashida = ParagraphAlignment.ArabicHighKashida,
        ArabicLowKashida = ParagraphAlignment.ArabicLowKashida,
        ThaiDistributed = ParagraphAlignment.ThaiDistributed
    }
}
