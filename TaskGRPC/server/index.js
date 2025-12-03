// cargar dependencias
const path = require("path");
const grpc = require("@grpc/grpc-js");
const protoLoader = require("@grpc/proto-loader");
const { ObjectId } = require("mongodb");
const { initDb, getCollection } = require("./db");
const { validateCreateRequest, validateUpdateRequest, validateId, validateBulkProject } = require("./validations");
const { docToProject, buildMongoFilters } = require("./utils");

// Cargar el archivo proto de definición del servicio gRPC
const PROTO_PATH = path.join(__dirname, "../proto/project.proto");
const packageDef = protoLoader.loadSync(PROTO_PATH, { keepCase: false });
const grpcObj = grpc.loadPackageDefinition(packageDef).project;

// Crea un nuevo proyecto
async function CreateProject(call, callback) {
  try {
    // extraer datos de la solicitud
    const req = call.request;
    
    // validaciones si no envia title o priority inválido
    const validatedData = validateCreateRequest(req);
    
    // preparar documento para inserción en la colección de la base de datos
    const coll = getCollection();
    const doc = {
      ...validatedData,
      status: "PENDING",
    };

    // validaciones con la base de datos si ya existe título
    const existing = await coll.findOne({ title: req.title });
    if (existing) {
      return callback({ code: grpc.status.ALREADY_EXISTS, message: "title ya existe" });
    }

    // insertar documento y devolver resultado
    const res = await coll.insertOne(doc);
    const stored = await coll.findOne({ _id: res.insertedId });
    return callback(null, docToProject(stored));
  } catch (err) {
    // manejo de errores
    if (err.code && err.message) {
      return callback(err);
    }
    console.error("Error:", err);
    return callback({ code: grpc.status.INTERNAL, message: "Error interno" });
  }
}

// update proyecto 
async function UpdateProject(call, callback) {
  try {
    // extraer datos de la solicitud
    const req = call.request;
    
    // validaciones con el id, title y priority
    const validatedData = validateUpdateRequest(req);

    // preparar actualización en la colección de la base de datos
    const coll = getCollection();

    // validacion por si otro documento ya tiene el mismo title
    const other = await coll.findOne({ title: req.title, _id: { $ne: validatedData.oid } });
    if (other) return callback({ code: grpc.status.ALREADY_EXISTS, message: "otro documento ya tiene el mismo title" });

    // preparar documento de actualización
    const updateDoc = {
      $set: {
        title: validatedData.title,
        // valores por defecto si faltan
        summary: validatedData.summary,
        priority: validatedData.priority,
        status: validatedData.status,
      },
    };

    // realizar actualización en la base de datos
    const updated = await coll.findOneAndUpdate(
      { _id: validatedData.oid },
      updateDoc,
      { returnDocument: "after" }
    );

    // si no se encontró el proyecto
    if (!updated) {
      return callback({
        code: grpc.status.NOT_FOUND,
        message: `Proyecto con id ${req.id} no encontrado`,
      });
    }

    // devolver el proyecto actualizado
    return callback(null, docToProject(updated));

  } 
  // manejo de errores
  catch (err) {
    if (err.code && err.message) {
      return callback(err);
    }
    console.error("Error:", err);
    return callback({ code: grpc.status.INTERNAL, message: "Error interno" });
  }
}

// delete proyecto
async function DeleteProject(call, callback) {
  try {
    // extraer datos de la solicitud
    const req = call.request;

    // validaciones con el id
    const oid = validateId(req.id);
    
    // preparar eliminación del proyect de la base de datos
    const coll = getCollection();
    
    // realizar eliminación del proyecto en la base de datos
    const res = await coll.deleteOne({ _id: oid });
    if (res.deletedCount === 0) return callback({ code: grpc.status.NOT_FOUND, message: `Proyecto con id ${req.id} no encontradao` });

    // devolver respuesta vacía
    return callback(null, {}); 
  } catch (err) {
    if (err.code && err.message) {
      return callback(err);
    }
    // manejo de errores
    console.error("Error:", err);
    return callback({ code: grpc.status.INTERNAL, message: "Error interno" });
  }
}

//obtener por id
async function GetProjectById(call, callback) {
  try {
    // extraer datos de la solicitud
    const req = call.request;
    
    // validaciones con el id
    const oid = validateId(req.id);

    // buscar el proyecto en la base de datos
    const coll = getCollection();

    // realizar búsqueda del proyecto en la base de datos
    const doc = await coll.findOne({ _id: oid });
    // si no se encontró el proyecto
    if (!doc) return callback({ code: grpc.status.NOT_FOUND, message: `Proyecto con id ${req.id} no encontrado` });

    // devolver el proyecto encontrado
    return callback(null, docToProject(doc));
  } catch (err) {
    if (err.code && err.message) {
      return callback(err);
    }
    // manejo de errores
    console.error("Error:", err);
    return callback({ code: grpc.status.INTERNAL, message: "Error interno" });
  }
}

//GetAll:ServerStreaming
async function ListProjects(call) {
  try {
    // preparar consulta de la base de datos
    const coll = getCollection();
    // construir filtro MongoDB a partir de los filtros recibidos
    const filters = call.request.filters || {};
    const mongoFilter = buildMongoFilters(filters);

    // ejecutar consulta 
    const cursor = coll.find(mongoFilter);

    // enviar resultados 
    for await (const doc of cursor) {
      call.write(docToProject(doc));
    }

    // finalizar el stream
    call.end();

  } catch (err) {
    // manejo de errores
    console.error("Error:", err);
    call.destroy(err);
  }
}

// BulkCreateProjects:ClientStreaming
async function BulkCreateProjects(call, callback) {
  const coll = getCollection();
  let created = 0;
  const docsToInsert = [];

  // manejar recepción de mensajes
  call.on("data", (req) => {
    // validaciones 
    const validatedDoc = validateBulkProject(req);
    if (validatedDoc) {
      // preparar array para mandar a la base de datos
      docsToInsert.push(validatedDoc);
    }
  });

  // terminar inserción al finalizar el stream
  call.on("end", async () => {
    if (docsToInsert.length === 0) {
      return callback(null, { 
        projects_created: 0,
        projects: [] 
      });
    }
        
    try {
      // verificar duplicados
      const titles = docsToInsert.map(doc => doc.title);
      const existing = await coll.find({ 
        title: { $in: titles } 
      }).toArray();
      
      // si hay duplicados, devolver error
      if (existing.length > 0) {
        const existingTitles = existing.map(doc => doc.title);
        return callback({
          code: grpc.status.ALREADY_EXISTS,
          message: `Título ya existe ${existingTitles.join(', ')}`
        });
      }
      
      
      // Si no hay duplicados, insertar
      const res = await coll.insertMany(docsToInsert);
      created = res.insertedCount;
      
      // Obtener los documentos insertados
      const insertedIds = Object.values(res.insertedIds);
      const storedDocs = await coll.find({ 
        _id: { $in: insertedIds } 
      }).toArray();
      
      // devolver resultado
      return callback(null, { 
        projects_created: created,
        projects: storedDocs.map(docToProject)
      });
      
    } catch (err) {
      console.error("Eerror", err);
      
      // manejar errores de duplicado
      if (err.code === 11000) {
        console.log("Error de duplicado en MongoDB");
        return callback({
          code: grpc.status.ALREADY_EXISTS,
          message: "Título ya existe en la base de datos"
        });
      }
      // manejar otros errores
      return callback({ 
        code: grpc.status.INTERNAL, 
        message: "Error interno del servidor" 
      });
    }
  });
}

// iniciar servidor gRPC
async function main() {
  await initDb();

  // crear servidor y registrar servicios
  const server = new grpc.Server();
  server.addService(grpcObj.ProjectService.service, {
    CreateProject,
    UpdateProject,
    DeleteProject,
    GetProjectById,
    ListProjects,
    BulkCreateProjects,
  });

  // arrancar servidor en el puerto especificado
  const port = process.env.GRPC_PORT || "50051";
  server.bindAsync(`0.0.0.0:${port}`, grpc.ServerCredentials.createInsecure(), () => {
    server.start();
    console.log(`GRPC server en 0.0.0.0:${port}`);
  });
}
// arrancar servidor y manejar errores
main().catch((err) => { console.error("Fallo al iniciar server:", err); process.exit(1); });