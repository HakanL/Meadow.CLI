﻿using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Meadow.CLI.Core;
using Meadow.CLI.Core.DeviceManagement;
using Meadow.CLI.Core.Devices;
using Microsoft.Extensions.Logging;

namespace Meadow.CLI.Commands
{
    public abstract class MeadowSerialCommand : ICommand, IDisposable
    {
        private protected ILogger Logger;
        private protected ILoggerFactory LoggerFactory;
        private protected MeadowDeviceManager MeadowDeviceManager;
        private protected MeadowDeviceHelper Meadow;

        private protected MeadowSerialCommand(ILoggerFactory loggerFactory, MeadowDeviceManager meadowDeviceManager)
        {
            LoggerFactory = loggerFactory;
            Logger = loggerFactory.CreateLogger<MeadowSerialCommand>();
            MeadowDeviceManager = meadowDeviceManager;
        }

        [CommandOption('v', Description = "Log verbosity")]
        public string[] Verbosity { get; init; }

        [CommandOption("SerialPort", 's', Description = "Meadow COM port")]
        public string SerialPortName
        {
            get => GetSerialPort();
            set => SetSerialPort(value);
        }

        //[CommandOption("listen", 'k', Description = "Keep port open to listen for output")]
        //public bool Listen {get; init;}

        private string _serialPort;

        private string GetSerialPort()
        {
            if (string.IsNullOrWhiteSpace(_serialPort))
            {
                // TODO: VALIDATE THE INPUT HERE, INPUT IS UNVALIDATED
                var port = SettingsManager.GetSetting(Setting.PORT);
                if (!string.IsNullOrWhiteSpace(port))
                    SerialPortName = port.Trim();
            }

            return _serialPort;
        }

        private void SetSerialPort(string value)
        {
            _serialPort = value;
            SettingsManager.SaveSetting(Setting.PORT, _serialPort);
        }

        public virtual async ValueTask ExecuteAsync(IConsole console)
        {
            Meadow?.Dispose();
            Meadow = null;

            var meadow = await MeadowDeviceManager.GetMeadowForSerialPort(SerialPortName, logger: Logger);

            if (meadow == null)
            {
                LoggerFactory.CreateLogger<MeadowSerialCommand>().LogCritical("Unable to find Meadow.");
                Environment.Exit(-1);
            }

            Meadow = new MeadowDeviceHelper(meadow, Logger);
        }

        public void Dispose()
        {
            LoggerFactory?.Dispose();
            Meadow?.Dispose();
        }
    }
}
