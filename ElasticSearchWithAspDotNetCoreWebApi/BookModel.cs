namespace ElasticSearchWithAspDotNetCore;

#pragma warning disable CS8618
public class BookModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Link { get; set; }

    public DateTime Date { get; set; }
}