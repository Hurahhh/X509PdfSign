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
        /// <summary>
        /// Get all where-to-sign in document
        /// </summary>
        /// <param name="pathToPdf"></param>
        /// <param name="signaturePlaceholder"></param>
        /// <returns></returns>
        public static IList<SignaturePosition> GetSignaturePositions(string pathToPdf, string signaturePlaceholder)
        {
            var result = new List<SignaturePosition>();

            var pdfDoc = new PdfDocument(new PdfReader(pathToPdf));
            var strategy = new LocationLetterExtractionStrategy();

            for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
            {
                var parser = new PdfCanvasProcessor(strategy);
                parser.ProcessPageContent(pdfDoc.GetPage(pageNum));

                var chunks = strategy.GetLetterChunks();
                var text = strategy.GetFullText().ToLower();

                var index = text.IndexOf(signaturePlaceholder.ToLower());
                if (index > 0)
                {
                    var first = chunks.ElementAt(index);
                    result.Add(new SignaturePosition(
                            pageNum,
                            first.Rect.GetX() + 10,
                            first.Rect.GetY() - 80
                    ));
                }
            }

            pdfDoc.Close();

            return result;
        }

        /// <summary>
        /// Get where-to-sign in a specific page of document
        /// </summary>
        /// <param name="pathToPdf"></param>
        /// <param name="pageNum">pass -1 if you want last page</param>
        /// <param name="signaturePlaceholder"></param>
        /// <returns></returns>
        public static SignaturePosition GetSignaturePosition(string pathToPdf, int pageNum, string signaturePlaceholder)
        {
            var pdfDoc = new PdfDocument(new PdfReader(pathToPdf));
            pageNum = pageNum == -1 ? pdfDoc.GetNumberOfPages() : pageNum;
            PdfPage page = pdfDoc.GetPage(pageNum);

            var strategy = new LocationLetterExtractionStrategy();
            var parser = new PdfCanvasProcessor(strategy);
            parser.ProcessPageContent(page);

            var chunks = strategy.GetLetterChunks();
            var text = strategy.GetFullText().ToLower();

            var index = text.IndexOf(signaturePlaceholder.ToLower());
            if (index > 0)
            {
                var first = chunks.ElementAt(index);
                return new SignaturePosition(
                        pageNum,
                        first.Rect.GetX() + 10,
                        first.Rect.GetY() - 80
                );
            }

            pdfDoc.Close();

            return null;
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