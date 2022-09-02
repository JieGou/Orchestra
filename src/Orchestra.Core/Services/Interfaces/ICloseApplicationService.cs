﻿namespace Orchestra.Services
{
    using System.Threading.Tasks;

    public interface ICloseApplicationService
    {
        Task CloseAsync();
        Task CloseAsync(bool force);
    }
}
