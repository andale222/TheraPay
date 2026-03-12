using TheraPay.Domain;

namespace TheraPay.Core;
public interface IPracticeDataStore
{
    PracticeData? Load();
    void Save(PracticeData data);
}