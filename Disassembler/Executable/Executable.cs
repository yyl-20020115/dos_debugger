namespace Disassembler;

public class Executable : Assembly
{
    public readonly ExecutableImage Image;
    public readonly Address EntryPoint;

    public Executable(string fileName)
    {
        var file = new MZFile(fileName);

        this.Image = new ExecutableImage(file);
        this.EntryPoint =
            new Address(Image.MapFrameToSegment(file.EntryPoint.Segment), file.EntryPoint.Offset);
    }

    public override BinaryImage GetImage() => Image;

    private static Address PointerToAddress(FarPointer pointer) => new(pointer.Segment, pointer.Offset);

    /// <summary>
    /// Gets the entry point address of the executable.
    /// </summary>

}

#if false
public class LoadModule : Module
{
    ImageChunk image;

    public LoadModule(ImageChunk image)
    {
        if (image == null)
            throw new ArgumentNullException("image");

        this.image = image;
    }

    /// <summary>
    /// Gets the binary image of the load module.
    /// </summary>
    public ImageChunk Image
    {
        get { return image; }
    }

    /// <summary>
    /// Gets or sets the initial value of SS register. This value must be
    /// relocated when the image is loaded.
    /// </summary>
    public UInt16 InitialSS { get; set; }

    /// <summary>
    /// Gets or sets the initial value of SP register.
    /// </summary>
    public UInt16 InitialSP { get; set; }

    /// <summary>
    /// Gets or sets the initial value of CS register. This value must be
    /// relocated when the image is loaded.
    /// </summary>
    public UInt16 InitialCS { get; set; }

    /// <summary>
    /// Gets or sets the initial value of IP register.
    /// </summary>
    public UInt16 InitialIP { get; set; }
}
#endif
