using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.System;
using OpenEpl.TextECode.Utils.Scopes;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal class SystemDataTypesHelper
    {
        public readonly SystemDataTypeElem Bin;
        public readonly SystemDataTypeElem Bool;
        public readonly SystemDataTypeElem Byte;
        public readonly SystemDataTypeElem DateTime;
        public readonly SystemDataTypeElem Double;
        public readonly SystemDataTypeElem Float;
        public readonly SystemDataTypeElem Int;
        public readonly SystemDataTypeElem Long;
        public readonly SystemDataTypeElem MethodPtr;
        public readonly SystemDataTypeElem Short;
        public readonly SystemDataTypeElem String;
        public readonly SystemDataTypeElem Any;
        public readonly SystemDataTypeElem Lambda;
        public readonly ImmutableDictionary<int, SystemDataTypeElem> IdMap;
        public SystemDataTypesHelper(TextECodeRestorer P)
        {
            Bin = new SystemDataTypeElem(P, EplSystemId.DataType_Bin, "字节集");
            Bool = new SystemDataTypeElem(P, EplSystemId.DataType_Bool, "逻辑型");
            Byte = new SystemDataTypeElem(P, EplSystemId.DataType_Byte, "字节型");
            DateTime = new SystemDataTypeElem(P, EplSystemId.DataType_DateTime, "日期时间型");
            Double = new SystemDataTypeElem(P, EplSystemId.DataType_Double, "双精度小数型");
            Float = new SystemDataTypeElem(P, EplSystemId.DataType_Float, "小数型");
            Int = new SystemDataTypeElem(P, EplSystemId.DataType_Int, "整数型");
            Long = new SystemDataTypeElem(P, EplSystemId.DataType_Long, "长整数型");
            MethodPtr = new SystemDataTypeElem(P, EplSystemId.DataType_MethodPtr, "子程序指针");
            Short = new SystemDataTypeElem(P, EplSystemId.DataType_Short, "短整数型");
            String = new SystemDataTypeElem(P, EplSystemId.DataType_String, "文本型");
            Any = new SystemDataTypeElem(P, EplSystemId.DataType_Any, "通用型");
            Lambda = new SystemDataTypeElem(P, EplSystemId.DataType_Lambda, "条件语句型");
            IdMap = ImmutableDictionary.CreateRange(new KeyValuePair<int, SystemDataTypeElem>[]
            {
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Bin, Bin),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Bool, Bool),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Byte, Byte),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_DateTime, DateTime),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Double, Double),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Float, Float),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Int, Int),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Long, Long),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_MethodPtr, MethodPtr),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Short, Short),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_String, String),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Any, Any),
                new KeyValuePair<int, SystemDataTypeElem>(EplSystemId.DataType_Lambda, Lambda)
            });
        }
        public bool TryGetById(int id, out SystemDataTypeElem elem) => IdMap.TryGetValue(id, out elem);
        public void AddToScope(IMutableScope<ProgramElemName, ProgramElem> scope)
        {
            foreach (var item in IdMap)
            {
                scope.Add(ProgramElemName.Type(item.Value.Name), item.Value);
            }
        }
    }
}
