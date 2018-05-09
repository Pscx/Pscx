using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Pscx.Commands.Text
{
    [OutputType(typeof(string[]), typeof(string))]
    [Cmdlet(VerbsCommon.Get, PscxNouns.LoremIpsum, DefaultParameterSetName = "Paragraph")]
    public class GetLoremIpsumCommand : PscxCmdlet
    {
        private const string ParamSetCharacter = "Character";
        private const string ParamSetWord = "Word";
        private const string ParamSetParagraph = "Paragraph";

        private string[] _currentLorem;
        private Dictionary<LoremIpsumLanguage, string[]> _lorem;
        private string[] _latin = {
            "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
            "Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.",
            "Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi.",
            "Nam liber tempor cum soluta nobis eleifend option congue nihil imperdiet doming id quod mazim placerat facer possim assum. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat.",
            "Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis.",
            "At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, At accusam aliquyam diam diam dolore dolores duo eirmod eos erat, et nonumy sed tempor et et invidunt justo labore Stet clita ea et gubergren, kasd magna no rebum. sanctus sea sed takimata ut vero voluptua. est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat.",
            "Consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus."
        } ;
        private string[] _silly = {
	        "Epsum factorial non deposit quid pro quo hic escorol. Olypian quarrels et gorilla congolium sic ad nauseum. Souvlaki ignitus carborundum e pluribus unum. Defacto lingo est igpay atinlay. Marquee selectus non provisio incongruous feline nolo contendre. Gratuitous octopus niacin, sodium glutimate. Quote meon an estimate et non interruptus stadium.",
	        "Sic tempus fugit esperanto hiccup estrogen. Glorious baklava ex librus hup hey ad infinitum. Non sequitur condominium facile et geranium incognito. Epsum factorial non deposit quid pro quo hic escorol. Marquee selectus non provisio incongruous feline nolo contendre Olypian quarrels et gorilla congolium sic ad nauseum. Souvlaki ignitus carborundum e pluribus unum.",
	        "Defacto lingo est igpay atinlay. Marquee selectus non provisio incongruous feline nolo contendre. Gratuitous octopus niacin, sodium glutimate. Quote meon an estimate et non interruptus stadium. Sic tempus fugit esperanto hiccup estrogen. Glorious baklava ex librus hup hey ad infinitum. Non sequitur condominium facile et geranium incognito. Epsum factorial non deposit quid pro quo hic escorol. Olypian quarrels et gorilla congolium sic ad nauseum. Souvlaki ignitus carborundum e pluribus unum. Defacto lingo est igpay atinlay. Gratuitous octopus niacin, sodium glutimate.",
	        "Quote meon an estimate et non interruptus stadium. Sic tempus fugit esperanto hiccup estrogen. Glorious baklava ex librus hup hey ad infinitum. Non sequitur condominium facile et geranium incognito goo goo ga joob."
        };
        private string[] _spanish = {
        	"Li Europan lingues es membres del sam familie. Lor separat existentie es un myth. Por scientie, musica, sport etc., li tot Europa usa li sam vocabularium. Li lingues differe solmen in li grammatica, li pronunciation e li plu commun vocabules. Omnicos directe al desirabilit&aacute; de un nov lingua franca: on refusa continuar payar custosi traductores. It solmen va esser necessi far uniform grammatica, pronunciation e plu sommun paroles."
        };
        private string[] _italian = {
         	"Ma quande lingues coalesce, li grammatica del resultant lingue es plu simplic e regulari quam ti del coalescent lingues. Li nov lingua franca va esser plu simplic e regulari quam li existent Europan lingues. It va esser tam simplic quam Occidental: in fact, it va esser Occidental. A un Angleso it va semblar un simplificat Angles, quam un skeptic Cambridge amico dit me que Occidental es."
        };

        public GetLoremIpsumCommand()
        {
            this.Length = 1;
            this.Language = LoremIpsumLanguage.Latin;

            _lorem = new Dictionary<LoremIpsumLanguage, string[]>();
            _lorem.Add(LoremIpsumLanguage.Latin, _latin);
            _lorem.Add(LoremIpsumLanguage.Silly, _silly);
            _lorem.Add(LoremIpsumLanguage.Spanish, _spanish);
            _lorem.Add(LoremIpsumLanguage.Italian, _italian);
        }

        [Parameter(Position = 0)]
        public int Length { get; set; }

        [Parameter(ParameterSetName = ParamSetCharacter)]
        public SwitchParameter Character { get; set; }

        [Parameter(ParameterSetName = ParamSetWord)]
        public SwitchParameter Word { get; set; }

        [Parameter(ParameterSetName = ParamSetParagraph)]
        public SwitchParameter Paragraph { get; set; }

        [Parameter]
        public LoremIpsumLanguage Language { get; set; }

        protected override void  BeginProcessing()
        {
            _currentLorem = _lorem[Language];
            if (this.ParameterSetName == ParamSetParagraph)
            {
                this.Paragraph = true;
            }
        }

        protected override void EndProcessing()
        {
            if (Paragraph)
            {
                var output = new List<string>();
                while (output.Count < Length)
                {
                    output.Add(_currentLorem[output.Count % _currentLorem.Length]);
                }
                WriteObject(String.Join("\n\n", output.ToArray()));
            }
            else if (Character)
            {
                var strBld = new StringBuilder();
                string loremStr = String.Join("\n\n", _currentLorem);
                while (strBld.Length < Length)
                {
                    strBld.AppendFormat(loremStr);
                }
                if (strBld.Length > Length)
                {
                    strBld.Length = Length;
                }
                WriteObject(strBld.ToString());
            }
            else if (Word)
            {
                int paraCount = 0;
                int wordCount = 0;

                var output = new List<string>();
                string[] wordList = _currentLorem[paraCount].Split(' ');

                while (output.Count < Length)
                {
                    if (wordCount >= wordList.Length)
                    {
                        wordCount = 0;
                        paraCount = (paraCount + 1)%_currentLorem.Length;
                        wordList = _currentLorem[paraCount].Split(' ');
                        wordList[0] = "\n\n" + wordList[0];
                    }
                    output.Add(wordList[wordCount++]);
                }

                string outputStr = String.Join(" ", output.ToArray());
                WriteObject(outputStr);
            }
        }
    }

    public enum LoremIpsumLanguage
    {
        Latin,
        Italian,
        Silly,
        Spanish
    }
}
