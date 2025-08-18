using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluation.Service.RepositoryFactory;

    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>() where T : class;
    }

