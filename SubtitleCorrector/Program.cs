using System.Text.RegularExpressions;

string inputFile = "napisy do filmu.srt";
string outputFile = "wyciete.srt";
TimeSpan timeShift = TimeSpan.FromSeconds(5).Add(TimeSpan.FromMilliseconds(880));

StreamReader inputStreamReader = new(inputFile);
(var subtitlesCopied, var subtitlesCutted) = SubtitlesTimeShifAndDivide(inputStreamReader, timeShift);
inputStreamReader.Close();

WriteSubtitlesFile(subtitlesCutted, outputFile);
WriteSubtitlesFile(subtitlesCopied, inputFile);

void WriteSubtitlesFile(List<Subtitle> subtitles, string path)
{
    StreamWriter outputStreamWriter = new(path);
    subtitles.ForEach((subtitle) =>
    {
        outputStreamWriter.WriteLine(subtitle.ToText());
    });
    outputStreamWriter.Close();
}

static (List<Subtitle> subtitlesCopied, List<Subtitle> subtitlesCutted) SubtitlesTimeShifAndDivide(StreamReader inputStreamReader, TimeSpan timeShift)
{
    int subtitleCopiedNewNumber = 0;
    int subtitleCuttedNewNumber = 0;

    List<Subtitle> subtitlesCopied = new();
    List<Subtitle> subtitlesCutted = new();
    List<string> buffer = new();
    string? line;

    while ((line = inputStreamReader.ReadLine()) != null)
    {
        buffer.Add(line);
        if (string.IsNullOrEmpty(line))
        {
            string subtitleNumberLine = buffer[0];
            string subtitleTimeLine = buffer[1];
            string subtitleText = string.Join("", buffer.Skip(2));

            var subtitle = new Subtitle(subtitleNumberLine, subtitleTimeLine, subtitleText, timeShift);

            if (subtitle.SubtitleStartTime.Milliseconds != 0)
            {

                subtitleCopiedNewNumber++;
                subtitle.SubtitleNumber = subtitleCopiedNewNumber;
                subtitlesCopied.Add(subtitle);
                buffer.Clear();
            }
            else
            {
                subtitleCuttedNewNumber++;
                subtitle.SubtitleNumber = subtitleCuttedNewNumber;
                subtitlesCutted.Add(subtitle);
                buffer.Clear();
            }
        }
    }
    return (subtitlesCopied, subtitlesCutted);
}

class Subtitle
{
    public int SubtitleNumber { get; set; }
    public TimeSpan SubtitleStartTime { get; set; }
    public TimeSpan SubtitleEndTime { get; set; }
    public String SubtitleText { get; set; }

    public Subtitle(string numberLine, string timeLine, string subtitle, TimeSpan offset)
    {
        SubtitleNumber = int.Parse(numberLine);
        (SubtitleStartTime, SubtitleEndTime) = ParseTimeSpan(timeLine);
        SubtitleStartTime += offset;
        SubtitleEndTime += offset;
        SubtitleText = subtitle;
    }

    public string ToText()
    {
        var timeLine = string.Format("{0} --> {1}", SubtitleStartTime.ToString(@"hh\:mm\:ss\,fff"), SubtitleEndTime.ToString(@"hh\:mm\:ss\,fff"));
        var result = string.Format("{0}\n{1}\n{2}\n", SubtitleNumber, timeLine, SubtitleText);
        return result;
    }

    internal (TimeSpan, TimeSpan) ParseTimeSpan(string timeString)
    {
        Match match = Regex.Match(timeString, @"(\d{2}):(\d{2}):(\d{2}),(\d{3})\s-->\s(\d{2}):(\d{2}):(\d{2}),(\d{3})");
        int hours = int.Parse(match.Groups[1].Value);
        int minutes = int.Parse(match.Groups[2].Value);
        int seconds = int.Parse(match.Groups[3].Value);
        int milliseconds = int.Parse(match.Groups[4].Value);
        var startTime = new TimeSpan(0, hours, minutes, seconds, milliseconds);

        hours = int.Parse(match.Groups[5].Value);
        minutes = int.Parse(match.Groups[6].Value);
        seconds = int.Parse(match.Groups[7].Value);
        milliseconds = int.Parse(match.Groups[8].Value);
        var EndTime = new TimeSpan(0, hours, minutes, seconds, milliseconds);

        return (startTime, EndTime);
    }
}