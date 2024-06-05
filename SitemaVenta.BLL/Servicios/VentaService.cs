using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;

namespace SistemaVenta.BLL.Servicios
{
    public class VentaService: IVentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IGenericRepository<DetalleVenta> _detVentaRepositorio;
        private readonly IMapper _mapper;

        public VentaService(IVentaRepository ventaRepository, IGenericRepository<DetalleVenta> detVentaRepositorio, IMapper mapper)
        {
            _ventaRepository = ventaRepository;
            _detVentaRepositorio = detVentaRepositorio;
            _mapper = mapper;
        }


        public async Task<VentaDTO> Registrar(VentaDTO modelo)
        {
            try
            {
                var ventaGenerada = await _ventaRepository.Registrar(_mapper.Map<Venta>(modelo));
                if (ventaGenerada.IdVenta == 0)
                {
                    throw new TaskCanceledException("No se pude realizar el registro.");
                }
                return _mapper.Map<VentaDTO>(ventaGenerada);
            } catch
            {
                throw;
            }
        }


        public async Task<List<VentaDTO>> Historial(string buscarPor, string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _ventaRepository.Consultar();
            var ListResultado = new List<Venta>();
            try
            {
                if(buscarPor == "fecha")
                {
                    DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-ES"));
                    DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-ES"));
                    ListResultado = await query.Where(v => 
                        v.FechaRegistro.Value.Date >= fech_inicio.Date &&
                        v.FechaRegistro.Value.Date <= fech_fin.Date)
                        .Include(dv => dv.DetalleVenta)
                        .ThenInclude(p => p.idProductoNavigation)
                        .ToListAsync();

                } else
                {
                    ListResultado = await query.Where(v => v.NumeroDocumento == numeroVenta)
                        .Include(dv => dv.DetalleVenta)
                        .ThenInclude(p => p.idProductoNavigation)
                        .ToListAsync();
                }
            }
            catch
            {
                throw;
            }

            return _mapper.Map<List<VentaDTO>>(ListResultado);
        }

        public async Task<List<ReporteDTO>> Reporte(string fechaInicio, string fechaFin)
        {
            IQueryable<DetalleVenta> query = await _detVentaRepositorio.Consultar();
            var ListResultado = new List<DetalleVenta>();
            try
            {
                DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-ES"));
                DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-ES"));
                ListResultado = await query
                    .Include(p => p.idProductoNavigation)
                    .Include(v => v.idVentaNavigation)
                    .Where(dv =>
                        dv.idVentaNavigation.FechaRegistro.Value.Date >= fech_inicio.Date &&
                        dv.idVentaNavigation.FechaRegistro.Value.Date <= fech_fin.Date
                    ).ToListAsync();
            }
            catch
            {
                throw;
            }
            return _mapper.Map<List<ReporteDTO>>(ListResultado);
        }
    }
}
