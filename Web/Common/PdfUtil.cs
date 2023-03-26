using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Collections.Generic;
using System.Linq;
using Web.Services;

namespace Web.Common
{
    public static class PdfUtil
    {
        public static IList<SignaturePosition> GetSignaturePositions(string pathToPdf, string signaturePlaceholder)
        {
            var signaturePositions = new List<SignaturePosition>();

            var pdfDoc = new PdfDocument(new PdfReader(pathToPdf));

            var strategy = new LocationLetterExtractionStrategy();

            for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
            {
                var page = pdfDoc.GetPage(pageNum);

                var parser = new PdfCanvasProcessor(strategy);
                parser.ProcessPageContent(page);

                var chunks = strategy.GetLetterChunks();
                var text = strategy.GetFullText();
                int index = text.IndexOf(signaturePlaceholder);
                if (index > 0)
                {
                    var first = chunks.ElementAt(index);

                    signaturePositions.Add(
                        new SignaturePosition(
                            pageNum,
                            first.Rect.GetX() + 10,
                            first.Rect.GetY() - 80
                        )
                    );
                }
            }

            pdfDoc.Close();

            return signaturePositions;
        }
    }

    public class LocationLetterExtractionStrategy : LocationTextExtractionStrategy
    {
        private bool isSorted = false;

        private IList<LetterChunk> letterChunks;

        public LocationLetterExtractionStrategy()
        {
            this.letterChunks = new List<LetterChunk>();
        }

        public override void EventOccurred(IEventData data, EventType type)
        {
            if (!type.Equals(EventType.RENDER_TEXT))
            {
                return;
            }

            var renderInfo = (TextRenderInfo)data;

            IList<TextRenderInfo> text = renderInfo.GetCharacterRenderInfos();
            foreach (TextRenderInfo t in text)
            {
                string letter = t.GetText();
                var letterStart = t.GetBaseline().GetStartPoint();
                var letterEnd = t.GetAscentLine().GetEndPoint();
                var letterRect = new Rectangle(letterStart.Get(0), letterStart.Get(1), letterEnd.Get(0) - letterStart.Get(0), letterEnd.Get(1) - letterStart.Get(1));

                if (!string.IsNullOrEmpty(letter))
                {
                    letterChunks.Add(new LetterChunk
                    {
                        Letter = letter,
                        Rect = letterRect
                    });
                }
            }
        }

        public IList<LetterChunk> GetLetterChunks()
        {
            if (this.isSorted)
            {
                return this.letterChunks;
            }

            return this.SortLetterChunks();
        }

        public IList<LetterChunk> SortLetterChunks()
        {
            this.isSorted = true;
            this.letterChunks = this.letterChunks
                                    .OrderByDescending(l => l.Rect.GetY())
                                    .ThenBy(l => l.Rect.GetX())
                                    .ToList();

            return this.letterChunks;
        }

        public string GetFullText()
        {
            var chunks = this.GetLetterChunks();

            return string.Join(string.Empty, chunks.Select(c => c.Letter));
        }
    }

    public class LetterChunk
    {
        public string Letter { get; set; }

        public Rectangle Rect { get; set; }
    }
}