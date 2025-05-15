using AutoMapper;
using Inventory.Application.DTOs.Warehouse;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;

namespace Inventory.Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default)
        {
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
        }

        public async Task<WarehouseDto?> GetWarehouseByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
            return warehouse != null ? _mapper.Map<WarehouseDto>(warehouse) : null;
        }

        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto createWarehouseDto, CancellationToken cancellationToken = default)
        {
            var warehouse = _mapper.Map<Warehouse>(createWarehouseDto);

            warehouse = await _unitOfWork.Warehouses.AddAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        public async Task UpdateWarehouseAsync(UpdateWarehouseDto updateWarehouseDto, CancellationToken cancellationToken = default)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(updateWarehouseDto.Id, cancellationToken);
            if (warehouse == null)
                throw new ApplicationException($"Warehouse with ID {updateWarehouseDto.Id} not found.");

            warehouse.Update(
                updateWarehouseDto.Name,
                updateWarehouseDto.Location,
                updateWarehouseDto.Address,
                updateWarehouseDto.ContactPerson,
                updateWarehouseDto.ContactEmail,
                updateWarehouseDto.ContactPhone
            );

            await _unitOfWork.Warehouses.UpdateAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteWarehouseAsync(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
            if (warehouse == null)
                throw new ApplicationException($"Warehouse with ID {id} not found.");

            // Check if there are inventory items in this warehouse
            var inventoryItems = await _unitOfWork.InventoryItems.GetByWarehouseIdAsync(id, cancellationToken);
            if (inventoryItems.Any())
                throw new ApplicationException($"Cannot delete warehouse with ID {id} because it contains inventory items.");

            await _unitOfWork.Warehouses.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ActivateWarehouseAsync(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
            if (warehouse == null)
                throw new ApplicationException($"Warehouse with ID {id} not found.");

            warehouse.Activate();
            await _unitOfWork.Warehouses.UpdateAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeactivateWarehouseAsync(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
            if (warehouse == null)
                throw new ApplicationException($"Warehouse with ID {id} not found.");

            warehouse.Deactivate();
            await _unitOfWork.Warehouses.UpdateAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
