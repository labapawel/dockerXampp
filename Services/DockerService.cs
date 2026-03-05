using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DockerManager.Services
{
    public class DockerService
    {
        private readonly DockerClient _client;

        public DockerService()
        {
            // Default to named pipe on Windows
            _client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();
        }

        public async Task<IList<ContainerListResponse>> ListContainersAsync()
        {
            return await _client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    All = true
                });
        }

        public async Task StartContainerAsync(string containerId)
        {
            await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
        }

        public async Task StopContainerAsync(string containerId)
        {
            await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        }

        public async Task RestartContainerAsync(string containerId)
        {
            await _client.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters());
        }

        public async Task RemoveContainerAsync(string containerId)
        {
            await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters()
            {
                Force = true,
                RemoveVolumes = true
            });
        }

        // Helper to pull image if not exists
        public async Task PullImageAsync(string image, string tag = "latest")
        {
            await _client.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = image,
                    Tag = tag
                },
                null,
                new Progress<JSONMessage>());
        }

        public async Task<string> CreateContainerAsync(string image, string tag, string name, HostConfig? hostConfig = null, IList<string>? env = null)
        {
             await PullImageAsync(image, tag);

             var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
             {
                 Image = $"{image}:{tag}",
                 Name = name,
                 HostConfig = hostConfig,
                 Env = env
             });

             return response.ID;
        }
    }
}
