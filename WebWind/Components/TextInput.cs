using System.Text.Json.Nodes;
using WebWind.Core;

namespace WebWind.Components;

public struct TextInputChangeEvent
{
    public string OldValue;
    public string NewValue;

    public TextInputChangeEvent(string oldValue, string newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public class TextInput : Component
{
    private List<Action<TextInputChangeEvent>> m_OnInputChangeEvents = new List<Action<TextInputChangeEvent>>();
    private string m_Value;

    public string Value => m_Value;
    
    public TextInput(string text = "")
    {
        m_Value = text;
        JS.CreateDocumentElement("input", Id);
        JS.Eval($"{Id}.type = 'text';");
        JS.Eval($"{Id}.value = `{text}`;");
        RegisterClientToServerEvent("input", $"data.value = {Id}.value;", (json) =>
        {
            var jsonObject = JsonNode.Parse(json).AsObject();
            string oldValue = Value;
            m_Value = jsonObject["value"].ToString();
            
            TextInputChangeEvent textInputChangeEvent = new TextInputChangeEvent(oldValue, m_Value);
            foreach (Action<TextInputChangeEvent> action in m_OnInputChangeEvents)
            {
                action(textInputChangeEvent);
            }
        });
    }

    public void AddOnInputChangeEvent(Action<TextInputChangeEvent> callback)
    {
        m_OnInputChangeEvents.Add(callback);
    }

}