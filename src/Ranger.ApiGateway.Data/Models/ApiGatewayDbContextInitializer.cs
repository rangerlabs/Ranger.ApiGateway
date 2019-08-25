using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.ApiGateway.Data {
    public class ApiGatewayDbContextInitializer : IApiGatewayDbContextInitializer {
        private readonly ApiGatewayDbContext context;

        public ApiGatewayDbContextInitializer (ApiGatewayDbContext context) {
            this.context = context;
        }

        public bool EnsureCreated () {
            return context.Database.EnsureCreated ();
        }

        public void Migrate () {
            context.Database.Migrate ();
        }
    }

    public interface IApiGatewayDbContextInitializer {
        bool EnsureCreated ();
        void Migrate ();
    }
}