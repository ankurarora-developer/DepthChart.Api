
namespace DepthChart.Common.Interfaces;

public interface ICorrelationLogger<T>
{
    void Info(string message);
    void Info(string message, object eventObject);
    void Debug(string message);
    void Debug(string message, object eventObject);
    void Error(string message);
    void Error(string message, object eventObject);
}
