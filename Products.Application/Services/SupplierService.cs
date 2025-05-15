using AutoMapper;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductNotificationService _notificationService;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, IProductNotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
        {
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
            return supplier != null ? _mapper.Map<SupplierDto>(supplier) : null;
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto, CancellationToken cancellationToken = default)
        {
            // Check if supplier with same email already exists
            if (await _unitOfWork.Suppliers.ExistsByEmailAsync(createSupplierDto.Email, cancellationToken))
                throw new ApplicationException($"Supplier with email '{createSupplierDto.Email}' already exists.");

            var supplier = _mapper.Map<Supplier>(createSupplierDto);

            supplier = await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var supplierDto = _mapper.Map<SupplierDto>(supplier);

            // Send real-time notification via SignalR
            await _notificationService.NotifySupplierCreated(supplierDto);

            return supplierDto;
        }

        public async Task UpdateSupplierAsync(UpdateSupplierDto updateSupplierDto, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(updateSupplierDto.Id, cancellationToken);
            if (supplier == null)
                throw new ApplicationException($"Supplier with ID {updateSupplierDto.Id} not found.");

            // Check if another supplier with the same email exists
            if (supplier.Email != updateSupplierDto.Email &&
                await _unitOfWork.Suppliers.ExistsByEmailAsync(updateSupplierDto.Email, cancellationToken))
                throw new ApplicationException($"Supplier with email '{updateSupplierDto.Email}' already exists.");

            supplier.Update(
                updateSupplierDto.Name,
                updateSupplierDto.ContactName,
                updateSupplierDto.Email,
                updateSupplierDto.Phone,
                updateSupplierDto.Address
            );

            await _unitOfWork.Suppliers.UpdateAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var supplierDto = _mapper.Map<SupplierDto>(supplier);

            // Send real-time notification via SignalR
            await _notificationService.NotifySupplierUpdated(supplierDto);
        }

        public async Task DeleteSupplierAsync(int id, CancellationToken cancellationToken = default)
        {
            // Check if supplier exists
            if (!await _unitOfWork.Suppliers.ExistsByIdAsync(id, cancellationToken))
                throw new ApplicationException($"Supplier with ID {id} not found.");

            // Check if there are products from this supplier
            var productsFromSupplier = await _unitOfWork.Products.GetBySupplierIdAsync(id, cancellationToken);
            if (productsFromSupplier.Any())
                throw new ApplicationException($"Cannot delete supplier with ID {id} because there are products associated with it.");

            await _unitOfWork.Suppliers.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notification via SignalR
            await _notificationService.NotifySupplierDeleted(id);
        }

        public async Task ActivateSupplierAsync(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
                throw new ApplicationException($"Supplier with ID {id} not found.");

            supplier.Activate();
            await _unitOfWork.Suppliers.UpdateAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var supplierDto = _mapper.Map<SupplierDto>(supplier);
            await _notificationService.NotifySupplierUpdated(supplierDto);
        }

        public async Task DeactivateSupplierAsync(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
            if (supplier == null)
                throw new ApplicationException($"Supplier with ID {id} not found.");

            supplier.Deactivate();
            await _unitOfWork.Suppliers.UpdateAsync(supplier, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var supplierDto = _mapper.Map<SupplierDto>(supplier);
            await _notificationService.NotifySupplierUpdated(supplierDto);
        }
    }
}
