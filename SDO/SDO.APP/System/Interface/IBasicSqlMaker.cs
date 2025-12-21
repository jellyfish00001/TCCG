namespace SDO.SqlMaker
{
    public interface IBasicSqlMaker
    {
        string Delete<T>(T model);
        string GetDataBaseName();
        string Insert<T>(T model);
        string Read<T>(T model);
        string SetDataBaseName(string name);
    }
}