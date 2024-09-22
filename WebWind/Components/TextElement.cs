using WebWind.Core;

namespace WebWind.Components;

public abstract class TextElement : Component
{
    private string _text;
    
    protected TextElement(string textString)
    {
        JS.CreateDocumentElement(GetTextElementTag(), Id);
        Text = textString;
    }
    
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            JS.Eval($"{Id}.innerText = '{_text}';");
        }
    }

    protected abstract string GetTextElementTag();
}