
namespace ShapeStats
{
    public interface IBuffable
    {
        void AddBuff(IBuff buff);
        void RemoveBuff(IBuff buff);
        void RemoveBuff(int id);
    }

}
