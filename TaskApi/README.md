# API SOAP de Gestión de Tareas

Este proyecto implementa una API SOAP para gestionar una lista de tareas por hacer, tomando en cuenta el título de la tarea, descripción, fecha límite, id y si esta completa o no. Está desarrollado en Python utilizando Spyne, SQLAlchemy y Alembic. 

## Requerimientos

- Docker y docker compose instalado y en ejecución.

## Cómo Levantar el Proyecto

Sigue estos pasos en orden desde la terminal.

1. Clonar el Repositorio

En tu terminal:

`git clone https://github.com/AnaRamirez60/SistemasDistribuidos2025.git`

`cd SistemasDistribuidos2025`

`cd TaskApi`

2. Levantar los Servicios

Ejecuta el siguiente comando desde la raíz de tu proyecto (donde se encuentra el archivo docker-compose.yml).

`pip3 install -r requirements.txt`

`docker-compose up -d`

¿Qué hace este comando?
Lee el Dockerfile, construye la imagen de la aplicación e instala todas las dependencias listadas en requirements.txt.

Descarga la imagen de mysql:8.0 y levanta el contenedor de la base de datos (TaskDb).

Levanta el contenedor de tu aplicación (taskservice) una vez que la base de datos esté disponible.

3. Ejecutar las Migraciones de la Base de Datos

Con los contenedores corriendo, se dbe crear las tablas en la base de datos. Esto se hace ejecutando el comando de alembic dentro del contenedor de la aplicación.

`alembic upgrade head`

Este comando le dice a Docker Compose que ejecute las migraciones de alembic upgrade head, creando la tabla tasks según los modelos de SQLAlchemy.

## Cómo Probar la API
Puedes usar cualquier cliente SOAP como Insomnia, Postman o SoapUI.

URL del WSDL: http://localhost:8000/task?wsdl

Con esta URL se importa el servicio al cliente. (Insomnia).

URL del endpoint del servicio para realizar peticiones: http://localhost:8000/task

Una vez importado, ya se pueden realizar las peticiones POST a esta URL.

## Instrucciones para Insomnia

En import, selecciona URL.

Pega la URL del WSDL (http://localhost:8000/task?wsdl) y haz clic en "Scan" y posteriormente en "import".

Insomnia generará automáticamente las plantillas para todas las operaciones (createTask, getTaskById, etc.).

La URL a la que se debe enviar la petición es la del endpoint (http://localhost:8000/task).

Rellena el cuerpo XML de la petición y envíala.

## Ejemplos de Peticiones SOAP en Insomnia

1. createTask

Crea una nueva tarea. El título y la fecha son obligatorios. Las fechas no pueden ser pasadas.

Ejemplo de body:

```<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">

 <soapenv:Body>

  <tns:createTask xmlns:tns="task.management.soap"><!-- mandatory -->

   <tns:title><!-- mandatory -->string</tns:title>

   <tns:description>string</tns:description>

   <tns:endDate><!-- mandatory -->2025-12-12</tns:endDate>

  </tns:createTask>

 </soapenv:Body>

</soapenv:Envelope>```

2. getTaskById

Busca una tarea específica por su ID numérico.

Ejemplo de body:

```<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">

 <soapenv:Body>

  <tns:getTaskById xmlns:tns="task.management.soap"><!-- mandatory -->

   <tns:task_id><!-- mandatory -->1</tns:task_id>

  </tns:getTaskById>

 </soapenv:Body>

</soapenv:Envelope>```

3. getTaskByTitle

Busca todas las tareas que contengan una letra o palabra en su título (no es sensible a mayúsculas y minúsculas).

Ejemplo de body:

```<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">

 <soapenv:Body>

  <tns:getTaskByTitle xmlns:tns="task.management.soap"><!-- mandatory -->

   <tns:title><!-- mandatory -->s</tns:title>

  </tns:getTaskByTitle>

 </soapenv:Body>

</soapenv:Envelope>```

## Ejemplos de Peticiones SOAP en Postman

1. createTask

Crea una nueva tarea. El título y la fecha son obligatorios. Las fechas no pueden ser pasadas.

Ejemplo de body:

```<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:tns="task.management.soap">

   <soapenv:Header/>

   <soapenv:Body>

      <tns:createTask>

         <tns:title>string</tns:title>

         <tns:description>string</tns:description>

         <tns:endDate>2025-12-12</tns:endDate>

      </tns:createTask>

   </soapenv:Body>

</soapenv:Envelope>```


2. getTaskById

Busca una tarea específica por su ID numérico.

Ejemplo de body:

```<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:tns="task.management.soap">

   <soapenv:Header/>

   <soapenv:Body>

      <tns:getTaskById>

         <tns:task_id>1</tns:task_id>

      </tns:getTaskById>

   </soapenv:Body>

</soapenv:Envelope>```


3. getTaskByTitle

Busca todas las tareas que contengan una letra o palabra en su título (no es sensible a mayúsculas y minúsculas).

Ejemplo de body:

```<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:tns="task.management.soap">

   <soapenv:Header/>

   <soapenv:Body>

      <tns:getTaskByTitle>

         <tns:title>s</tns:title>

      </tns:getTaskByTitle>

   </soapenv:Body>

</soapenv:Envelope>```
