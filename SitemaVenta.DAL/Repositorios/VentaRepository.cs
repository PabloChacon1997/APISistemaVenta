﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DAL.DBontext;
using SistemaVenta.Model;

namespace SistemaVenta.DAL.Repositorios
{
    public class VentaRepository: GenericRepository<Venta>, IVentaRepository
    {
        private readonly DbventaContext _dbcontext;

        public VentaRepository(DbventaContext dbcontext): base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Venta> Registrar(Venta modelo)
        {
            Venta ventaGenerada = new Venta();
            using(var transaction = _dbcontext.Database.BeginTransaction())
            try
            {
                foreach (DetalleVenta dv in modelo.DetalleVenta)
                {
                    Producto producto_encontrado = _dbcontext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();
                    producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad;
                    _dbcontext.Productos.Update(producto_encontrado);
                }
                await _dbcontext.SaveChangesAsync();
                Numerodocumento correlativo = _dbcontext.Numerodocumentos.First();
                correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                correlativo.FechaRegistro = DateTime.Now;

                _dbcontext.Numerodocumentos.Update(correlativo);
                await _dbcontext.SaveChangesAsync();

                int CantidadDigistos = 4;
                string ceros = string.Concat(Enumerable.Repeat("0", CantidadDigistos));
                string numeroVenta = ceros + correlativo.UltimoNumero.ToString();

                numeroVenta = numeroVenta.Substring(numeroVenta.Length - CantidadDigistos, CantidadDigistos);

                modelo.NumeroDocumento = numeroVenta;
                await _dbcontext.Venta.AddAsync(modelo);
                await _dbcontext.SaveChangesAsync();

                ventaGenerada = modelo;
                transaction.Commit();
            } catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }

            return ventaGenerada;
        }
    }
}