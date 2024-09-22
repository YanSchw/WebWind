using WebWind.Core;

namespace WebWind.Components;

public class Paragraph : TextElement
{
    public Paragraph(string text) : base(text)
    {
        
    }
    
    protected override string GetTextElementTag()
    {
        return "p";
    }
}