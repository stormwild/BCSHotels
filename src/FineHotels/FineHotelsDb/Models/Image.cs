namespace FineHotelsDb.Models;

public class Image
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public long HotelId { get; set; }

    public virtual Hotel Hotel { get; set; }

}
