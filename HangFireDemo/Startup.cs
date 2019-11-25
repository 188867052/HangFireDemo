using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HangFireDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var hangfireConnStr = Configuration.GetSection("HangfireConnectionString").Value;
            services.AddHangfire(configuration => configuration.UseSqlServerStorage(hangfireConnStr));
            services.AddSingleton<IMessageService, MessageService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // ����ִ�еĵ�������
            BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget"));
            // �����ƻ�����,5���Ӻ�ִ��
            BackgroundJob.Schedule(() => Console.WriteLine("Delayed"), TimeSpan.FromMinutes(1));
            // ��������������,ÿ��1����ִ��һ��
            RecurringJob.AddOrUpdate<IMessageService>(o => o.Test(), Cron.Minutely());
            // ��������������,ÿ��21:10ִ��,
            RecurringJob.AddOrUpdate("JobB", () => Console.WriteLine(DateTimeOffset.Now), "10 21 * * *");
            // ��������������,ÿ��21:10ִ��
            RecurringJob.AddOrUpdate("JobC", () => Test(), "10 21 * * *", TimeZoneInfo.Local);
        }

        public void Test()
        {
            Console.WriteLine(DateTimeOffset.Now);
        }
    }
}
