using System.Collections;
public interface ITrap
{
    trapState state { get; set; }
    IEnumerator trap(float speedMult, int duration);
    IEnumerator reset();
}