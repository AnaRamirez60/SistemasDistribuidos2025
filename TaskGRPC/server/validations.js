// cargar dependencias
const { ObjectId } = require("mongodb");
const grpc = require("@grpc/grpc-js");

// validaciones si no envia title o priority inválido
function validateCreateRequest(req) {
  if (!req.title || req.title.trim() === "") {
    throw { code: grpc.status.INVALID_ARGUMENT, message: "title es obligatorio" };
  }
  if (!(req.priority >= 1 && req.priority <= 5)) {
    throw { code: grpc.status.INVALID_ARGUMENT, message: "priority debe ser entre 1 y 5" };
  }
  return {
    title: req.title,
    summary: req.summary || "",
    priority: req.priority
  };
}

// validaciones con el id, title y priority
function validateUpdateRequest(req) {
  if (!req.id) throw { code: grpc.status.INVALID_ARGUMENT, message: "id requerido" };
  if (!req.title || req.title.trim() === "") throw { code: grpc.status.INVALID_ARGUMENT, message: "title es obligatorio" };
  if (!req.summary || req.summary.trim() === "") throw { code: grpc.status.INVALID_ARGUMENT, message: "summary es obligatorio" };
  if (!req.status || req.status.trim() === "") throw { code: grpc.status.INVALID_ARGUMENT, message: "status es obligatorio" };
  if (!(req.priority >= 1 && req.priority <= 5)) {
    throw { code: grpc.status.INVALID_ARGUMENT, message: "priority debe ser entre 1 y 5" };
  }
  // convertir id a ObjectId para consulta
  let oid;
  try { 
    oid = new ObjectId(req.id); 
  } catch {
    throw { code: grpc.status.INVALID_ARGUMENT, message: "id formato inválido" };
  }

  // retornar objeto validado
  return {
    oid,
    title: req.title,
    summary: req.summary || "",
    priority: req.priority,
    status: req.status || "PENDING"
  };
}

// validaciones con el id
function validateId(id) {
  if (!id) throw { code: grpc.status.INVALID_ARGUMENT, message: "id requerido" };
  
  // convertir id a ObjectId para consulta
  let oid;
  try { 
    oid = new ObjectId(id); 
  } catch {
    throw { code: grpc.status.INVALID_ARGUMENT, message: "id formato inválido" };
  }
  return oid;
}

// validaciones para bulk create
function validateBulkProject(req) {
  // validaciones 
  if (!req.title || req.title.trim() === "") {
    // ignorar items inválidos 
    console.log("Proyecto sin título");
    return null;
  }
  // validar priority y asignar valor por defecto si es inválido
  const p = (req.priority >= 1 && req.priority <= 5) ? req.priority : 1;
  
  return {
    title: req.title,
    summary: req.summary || "",
    priority: p,
    status: "BULK_CREATED",
  };
}
// exportar funciones para ser utilizadas por index.js
module.exports = {
  validateCreateRequest,
  validateUpdateRequest,
  validateId,
  validateBulkProject
};