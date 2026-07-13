//Interface that defines an entity capable of becoming invisible
public interface IInvisibility 
{
    public bool CanGoInvisible { get; set; }
    public void GoInvisible(float duration);
}
