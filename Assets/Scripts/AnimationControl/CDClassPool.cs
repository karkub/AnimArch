using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public class CDClassPool
    {
        private long ClassIDPrefix;
        public List<CDClass> Classes { get; }

        public CDClassPool()
        {
            this.Classes = new List<CDClass>();
        }

        public CDClassInstance GetClassInstanceById(String ClassName, long Id)
        {
            CDClassInstance Result = null;
            foreach (CDClass Class in this.Classes)
            {
                if (String.Equals(Class.Name, ClassName))
                {
                    Result = Class.GetInstanceByID(Id);
                    break;
                }
            }

            return Result; 
        }
        public CDClassInstance GetClassInstanceById(long Id)
        {
            CDClassInstance Result = null;
            foreach (CDClass Class in this.Classes)
            {
                Result = Class.GetInstanceByID(Id);
                if (Result != null)
                {
                    break;
                }
            }

            return Result;
        }
        public CDClass SpawnClass(String Name)
        {
            CDClass NewClass = null;
            if (!ClassExists(Name))
            {
                NewClass = new CDClass(Name, this);
                this.Classes.Add(NewClass);
            }
            return NewClass;
        }

        public Boolean ClassesExist(List<String> ClassNames)
        {
            Boolean Result = true;

            foreach (String ClassName in ClassNames)
            {
                if (!ClassExists(ClassName))
                {
                    Result = false;
                }
            }

            return Result;
        }

        public Boolean ClassExists(String ClassName)
        {
            Boolean Result = false;

            foreach (CDClass Class in this.Classes)
            {
                if (Class.Name.Equals(ClassName))
                {
                    Result = true;
                    break;
                }
            }

            return Result;
        }

        public CDClass getClassByName(String ClassName)
        {
            CDClass Result = null;

            foreach (CDClass Class in this.Classes)
            {
                if (Class.Name.Equals(ClassName))
                {
                    Result = Class;
                    break;
                }
            }

            return Result;
        }

        public Boolean MethodExists(String ClassName, String MethodName, bool includeInherited = false)
        {
            if (!ClassExists(ClassName))
            {
                return false;
            }

            CDClass SearchedClass = null;

            foreach (CDClass Class in this.Classes)
            {
                if (Class.Name.Equals(ClassName))
                {
                    SearchedClass = Class;
                    break;
                }
            }

            if (SearchedClass == null)
            {
                return false;
            }

            return SearchedClass.MethodExists(MethodName, includeInherited);
        }
        public EXEExecutionResult CreateInstance(string className)
        {
            if (!ClassExists(className))
            {
                return EXEExecutionResult.Error("XEC2023", ErrorMessage.ClassNotFound(className, this));
            }

            CDClass targetClass = getClassByName(className);
            CDClassInstance createdInstance = targetClass.CreateClassInstance();

            EXEExecutionResult result = EXEExecutionResult.Success();
            result.ReturnedOutput = new EXEValueReference(createdInstance);

            return result;
        }
        public bool DestroyInstance(String ClassName, long InstanceId)
        {
            return getClassByName(ClassName).DestroyInstance(InstanceId);
        }
        public bool DestroyInstance(CDClass Class, long InstanceId)
        {
            return Class.DestroyInstance(InstanceId);
        }

        public Dictionary<String, long> ProduceInstanceDatabase()
        {
            Dictionary<String, long> InstanceDatabase = new Dictionary<String, long>();
            foreach (CDClass Class in this.Classes)
            {
                Class.AppendToInstanceDatabase(InstanceDatabase);
            }
            return InstanceDatabase;
        }

        public Dictionary<String, int> ProduceInstanceHistogram()
        {
            Dictionary<String, int> InstanceHistogram = new Dictionary<String, int>();
            foreach (CDClass Class in this.Classes)
            {
                InstanceHistogram.Add(Class.Name, Class.InstanceCount());
            }
            return InstanceHistogram;
        }
    }
}