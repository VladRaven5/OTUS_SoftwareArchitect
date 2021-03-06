using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace LabelsService
{
    public class LabelsManager : DomainManagerBase
    {
        private readonly LabelsRepository _labelsRepository;
        private readonly RequestsRepository _requestsRepository;

        public LabelsManager(LabelsRepository labelsRepository, RequestsRepository requestsRepository)
            : base(requestsRepository)
        {
            _labelsRepository = labelsRepository;
            _requestsRepository = requestsRepository;
        }

        public Task<IEnumerable<LabelModel>> GetAllLabelsAsync()
        {
            return _labelsRepository.GetLabelsAsync();
        }

        public async Task<LabelModel> GetLabelByIdAsync(string labelId)
        {
             var label = await _labelsRepository.GetLabelByIdAsync(labelId);
             if(label == null)
             {
                 throw new NotFoundException($"Label with id {labelId} not found");
             }
             return label;
        }

        public async Task<LabelModel> CreateLabelAsync(LabelModel newLabel, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                newLabel.Init();

                var outboxMessage = OutboxMessageModel.Create(
                    new LabelCreatedUpdatedMessage
                    {
                        LabelId = newLabel.Id,
                        Title = newLabel.Title,
                        Color = newLabel.Color
                    }, Topics.Labels, MessageActions.Created);

                return await _labelsRepository.CreateLabelAsync(newLabel, outboxMessage);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }
        }

        public async Task<LabelModel> UpdateLabelAsync(LabelModel updatingLabel)
        {
            LabelModel currentLabel = await _labelsRepository.GetLabelByIdAsync(updatingLabel.Id);
            if(currentLabel == null)
            {
                throw new NotFoundException($"Label with id = {updatingLabel.Id} not found");
            }

            if(currentLabel.Version != updatingLabel.Version)
            {
                throw new VersionsNotMatchException();
            }

            var outboxMessage = OutboxMessageModel.Create(
                new LabelCreatedUpdatedMessage
                {
                    LabelId = updatingLabel.Id, 
                    OldTitle = currentLabel.Title,                   
                    Title = updatingLabel.Title,
                    Color = updatingLabel.Color
                }, Topics.Labels, MessageActions.Updated);

            return await _labelsRepository.UpdateLabelAsync(updatingLabel, outboxMessage);
        }
        public async Task DeleteLabelAsync(string labelId)
        {
            var label = await _labelsRepository.GetLabelByIdAsync(labelId);
            if(label == null)
                return;

            var outboxMessage = OutboxMessageModel.Create(
                new LabelDeletedMessage
                {
                    LabelId = labelId,
                    Title = label.Title
                }, Topics.Labels, MessageActions.Deleted);

            await _labelsRepository.DeleteLabelAsync(labelId, outboxMessage);
        }
    }
}